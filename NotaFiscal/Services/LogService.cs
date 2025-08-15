namespace NotaFiscal.Services
{
    public class LogService
    {
        private string _logFileName = string.Empty;

        /// <summary>
        /// Inicializa o sistema de log com base no nome da planilha
        /// </summary>
        /// <param name="nomeArquivoPlanilha">Nome do arquivo da planilha</param>
        /// <returns>Caminho do arquivo de log criado ou string vazia se houver erro</returns>
        public string InicializarLog(string nomeArquivoPlanilha)
        {
            try
            {
                var nomeArquivo = Path.GetFileNameWithoutExtension(nomeArquivoPlanilha);
                var dataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _logFileName = $"Log_{nomeArquivo}_{dataHora}.txt";
                
                // Tenta diferentes localizações para a pasta de logs
                string logsPath;
                
                // Primeiro tenta usar o diretório da aplicação
                var appPath = AppContext.BaseDirectory;
                logsPath = Path.Combine(appPath, "Logs");
                
                // Se não funcionar, tenta o diretório atual
                if (!TryCreateDirectory(logsPath))
                {
                    appPath = Directory.GetCurrentDirectory();
                    logsPath = Path.Combine(appPath, "Logs");
                    
                    // Se ainda não funcionar, usa o diretório temp do usuário
                    if (!TryCreateDirectory(logsPath))
                    {
                        logsPath = Path.Combine(Path.GetTempPath(), "NotaFiscal_Logs");
                        TryCreateDirectory(logsPath);
                    }
                }
                
                var caminhoLog = Path.Combine(logsPath, _logFileName);
                
                // Cria o arquivo de log inicial
                File.WriteAllText(caminhoLog, $"=== LOG DE IMPORTAÇÃO DA PLANILHA ===\n");
                File.AppendAllText(caminhoLog, $"Arquivo: {nomeArquivoPlanilha}\n");
                File.AppendAllText(caminhoLog, $"Data/Hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
                File.AppendAllText(caminhoLog, $"Localização do Log: {caminhoLog}\n");
                File.AppendAllText(caminhoLog, $"========================================\n\n");
                
                return caminhoLog;
            }
            catch (Exception ex)
            {
                // Se falhar ao criar o log, apenas continua sem logging
                _logFileName = string.Empty;
                System.Diagnostics.Debug.WriteLine($"Erro ao inicializar log: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Registra uma mensagem no arquivo de log
        /// </summary>
        /// <param name="mensagem">Mensagem a ser registrada</param>
        public void RegistrarLog(string mensagem)
        {
            if (string.IsNullOrEmpty(_logFileName))
                return;
                
            try
            {
                var caminhoLog = GetLogFilePath();
                if (!string.IsNullOrEmpty(caminhoLog))
                {
                    var logEntry = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {mensagem}\n";
                    File.AppendAllText(caminhoLog, logEntry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar log: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra um erro específico com detalhes da linha que causou o problema
        /// </summary>
        /// <param name="numeroLinha">Número da linha da planilha</param>
        /// <param name="erro">Mensagem de erro</param>
        /// <param name="dadosLinha">Dados da linha que causaram o erro (opcional)</param>
        public void RegistrarErroLog(int numeroLinha, string erro, string dadosLinha = "")
        {
            var mensagem = $"ERRO - Linha {numeroLinha}: {erro}";
            if (!string.IsNullOrEmpty(dadosLinha))
            {
                mensagem += $" | Dados: {dadosLinha}";
            }
            RegistrarLog(mensagem);
        }

        /// <summary>
        /// Obtém o caminho completo do arquivo de log atual
        /// </summary>
        /// <returns>Caminho do arquivo de log ou string vazia se não encontrado</returns>
        public string GetLogFilePath()
        {
            if (string.IsNullOrEmpty(_logFileName))
                return string.Empty;

            // Tenta encontrar o arquivo de log nas possíveis localizações
            var possiveisCaminhos = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Logs", _logFileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Logs", _logFileName),
                Path.Combine(Path.GetTempPath(), "NotaFiscal_Logs", _logFileName)
            };

            foreach (var caminho in possiveisCaminhos)
            {
                if (File.Exists(caminho))
                {
                    return caminho;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Testa o sistema de logs criando um arquivo de teste
        /// </summary>
        /// <returns>Resultado do teste</returns>
        public string TestarSistemaLogs()
        {
            try
            {
                var caminhoLog = InicializarLog("teste_planilha.xlsx");
                RegistrarLog("Teste do sistema de logs - funcionando!");
                
                if (!string.IsNullOrEmpty(caminhoLog) && File.Exists(caminhoLog))
                {
                    return $"Log de teste criado com sucesso em: {caminhoLog}";
                }
                else
                {
                    return "Erro: Não foi possível criar o arquivo de log";
                }
            }
            catch (Exception ex)
            {
                return $"Erro ao testar logs: {ex.Message}";
            }
        }

        /// <summary>
        /// Tenta criar um diretório, retornando true se bem-sucedido
        /// </summary>
        /// <param name="path">Caminho do diretório</param>
        /// <returns>True se o diretório foi criado ou já existe, false caso contrário</returns>
        private bool TryCreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
