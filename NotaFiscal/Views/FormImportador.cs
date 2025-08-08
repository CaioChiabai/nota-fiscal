using NotaFiscal.Controllers;
using NotaFiscal.Data;
using Microsoft.EntityFrameworkCore;

namespace NotaFiscal.Views
{
    public partial class FormImportador : Form
    {
        private readonly PlanilhaController _planilhaController;
        private readonly AppDbContext _context;

        public FormImportador(PlanilhaController planilhaController, AppDbContext context)
        {
            InitializeComponent();
            _planilhaController = planilhaController;
            _context = context;
            
            // Associa o evento Load para verificar conexão ao inicializar
            this.Load += FormImportador_Load;
        }

        private async void FormImportador_Load(object? sender, EventArgs e)
        {
            AppendLog("Iniciando aplicação...");
            await VerificarConexaoBanco();
        }

        private async Task VerificarConexaoBanco()
        {
            try
            {
                AppendLog("Verificando conexão com o banco de dados...");
                
                // Tenta abrir uma conexão e fazer uma consulta simples
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Verifica se o banco existe e se há tabelas
                    await _context.Database.EnsureCreatedAsync();
                    
                    AppendLog("✓ Conexão com banco de dados estabelecida com sucesso!");
                    AppendLog($"✓ Banco de dados configurado e funcionando");
                }
                else
                {
                    AppendLog("✗ Falha ao conectar com o banco de dados");
                    AppendLog("⚠ Verifique a string de conexão no arquivo appsettings.json");
                }
            }
            catch (Exception ex)
            {
                AppendLog("✗ Erro ao verificar conexão com banco de dados:");
                AppendLog($"  {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    AppendLog($"  Detalhe: {ex.InnerException.Message}");
                }
                
                AppendLog("⚠ Verifique se:");
                AppendLog("  - O SQL Server está executando");
                AppendLog("  - A string de conexão está correta");
                AppendLog("  - As permissões de acesso estão configuradas");
            }
        }


        private void btnSelecionarPlanilha_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Planilhas Excel (*.xlsx)|*.xlsx";
            if (dialog.ShowDialog() == DialogResult.OK)
                txtBoxCaminhoPlanilha.Text = dialog.FileName;
        }

        private async void btnImportarPlanilha_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBoxCaminhoPlanilha.Text))
            {
                AppendLog("Atenção: selecione uma planilha para importar.");
                return;
            }

            try
            {
                btnImportarPlanilha.Enabled = false;
                btnImportarPlanilha.Text = "Importando...";

                AppendLog($"Arquivo selecionado: {Path.GetFileName(txtBoxCaminhoPlanilha.Text)}");

                // Encaminha logs do controller para o TextBox (sincronizado no thread de UI)
                var progress = new Progress<string>(msg => AppendLog(msg));

                await _planilhaController.ImportarPlanilha(txtBoxCaminhoPlanilha.Text, progress);

                AppendLog("Importação finalizada com sucesso.");
            }
            catch (Exception ex)
            {
                AppendLog($"ERRO: {ex.Message}");
                if (ex.InnerException != null)
                    AppendLog($"Detalhe: {ex.InnerException.Message}");
            }
            finally
            {
                btnImportarPlanilha.Enabled = true;
                btnImportarPlanilha.Text = "Importar";
            }
        }

        private void AppendLog(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            txtBoxLogs.AppendText(line);
        }
    }
}
