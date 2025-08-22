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
                
                // Define o caminho fixo para a pasta Logs dentro do projeto
                var projectPath = Path.GetDirectoryName(Path.GetDirectoryName(AppContext.BaseDirectory));
                if (projectPath != null)
                {
                    // Sobe mais um nível para sair da pasta bin/Debug/net8.0-windows
                    projectPath = Path.GetDirectoryName(projectPath);
                }
                
                string logsPath = Path.Combine(projectPath ?? Directory.GetCurrentDirectory(), "Logs");
                TryCreateDirectory(logsPath);
                
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
        /// Registra um erro específico com detalhes do campo que causou o problema
        /// </summary>
        /// <param name="numeroLinha">Número da linha da planilha</param>
        /// <param name="nomeCampo">Nome do campo que causou o erro</param>
        /// <param name="valorCampo">Valor do campo que causou o erro</param>
        /// <param name="erroDetalhado">Mensagem de erro detalhada</param>
        /// <param name="dadosCompletos">Dados completos da linha (opcional)</param>
        public void RegistrarErroCampo(int numeroLinha, string nomeCampo, string valorCampo, string erroDetalhado, string dadosCompletos = "")
        {
            // Converte o valor para string se não for nulo
            string valorString = valorCampo?.ToString() ?? "null";
            
            var mensagem = $"Linha {numeroLinha} - Campo: {nomeCampo} - Valor: '{valorString}' - Erro: {erroDetalhado}";
            
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

            // Define o caminho fixo para a pasta Logs dentro do projeto
            var projectPath = Path.GetDirectoryName(Path.GetDirectoryName(AppContext.BaseDirectory));
            if (projectPath != null)
            {
                projectPath = Path.GetDirectoryName(projectPath);
            }
            
            var logsPath = Path.Combine(projectPath ?? Directory.GetCurrentDirectory(), "Logs");
            var caminhoLog = Path.Combine(logsPath, _logFileName);
            
            return File.Exists(caminhoLog) ? caminhoLog : string.Empty;
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
