using NotaFiscal.Controllers;
using NotaFiscal.Data;

namespace NotaFiscal.Views
{
    public partial class FormImportador : Form
    {
        private readonly PlanilhaController _planilhaController;
        private readonly AppDbContext _context;

        private System.Windows.Forms.Timer? _statusTimer;
        private bool _checandoStatus = false;

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
            IniciarMonitoramentoStatus();
        }
        private void IniciarMonitoramentoStatus()
        {
            _statusTimer = new System.Windows.Forms.Timer();
            _statusTimer.Interval = 1000; // 1s
            _statusTimer.Tick += async (_, __) => await ChecarStatusRapido();
            _statusTimer.Start();
        }

        private async Task ChecarStatusRapido()
        {
            if (_checandoStatus) return;
            _checandoStatus = true;
            try
            {
                bool ok = await _context.Database.CanConnectAsync();
                AtualizarIndicador(ok, silencioso: true);
            }
            catch
            {
                AtualizarIndicador(false, silencioso: true);
            }
            finally
            {
                _checandoStatus = false;
            }
        }

        private DateTime? _ultimoDesconectado;
        private void AtualizarIndicador(bool online, bool silencioso = false)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AtualizarIndicador(online, silencioso));
                return;
            }

            if (online)
            {
                lblDbStatus.Text = "● Conectado";
                lblDbStatus.ForeColor = Color.ForestGreen;
                _ultimoDesconectado = null;
            }
            else
            {
                if (_ultimoDesconectado == null)
                    _ultimoDesconectado = DateTime.Now;
                lblDbStatus.Text = $"● Desconectado desde {_ultimoDesconectado:HH:mm:ss}";
                lblDbStatus.ForeColor = Color.Firebrick;
            }

            if (!silencioso)
                AppendLog(online ? "Status banco: ONLINE" : "Status banco: OFFLINE");
        }

        private async Task VerificarConexaoBanco()
        {
            try
            {
                AppendLog("Verificando conexão com o banco de dados...");
                var canConnect = await _context.Database.CanConnectAsync();
                AtualizarIndicador(canConnect);
                if (canConnect)
                {
                    await _context.Database.EnsureCreatedAsync();
                    AppendLog("✓ Conexão com banco de dados estabelecida com sucesso!");
                    AppendLog("✓ Banco de dados configurado e funcionando");
                }
                else
                {
                    AppendLog("✗ Falha ao conectar com o banco de dados");
                    AppendLog("⚠ Verifique a string de conexão no arquivo appsettings.json");
                }
            }
            catch (Exception ex)
            {
                AtualizarIndicador(false);
                AppendLog("✗ Erro ao verificar conexão com banco de dados:");
                AppendLog($"  {ex.Message}");
                if (ex.InnerException != null)
                    AppendLog($"  Detalhe: {ex.InnerException.Message}");
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
