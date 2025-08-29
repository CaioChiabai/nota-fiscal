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
        private DateTime? _ultimoDesconectado;

        public FormImportador(PlanilhaController planilhaController, AppDbContext context)
        {
            InitializeComponent();
            _planilhaController = planilhaController;
            _context = context;
            this.Load += FormImportador_Load;
        }

        private async void FormImportador_Load(object? sender, EventArgs e)
        {
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
                AtualizarIndicador(ok);
            }
            catch
            {
                AtualizarIndicador(false);
            }
            finally
            {
                _checandoStatus = false;
            }
        }

        private void AtualizarIndicador(bool online)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AtualizarIndicador(online));
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
        }

        private async Task VerificarConexaoBanco()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                AtualizarIndicador(canConnect);
            }
            catch
            {
                AtualizarIndicador(false);
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
                return;

            try
            {
                btnImportarPlanilha.Enabled = false;
                btnImportarPlanilha.Text = "Importando...";

                await _planilhaController.ImportarPlanilha(txtBoxCaminhoPlanilha.Text);
            }
            finally
            {
                btnImportarPlanilha.Enabled = true;
                btnImportarPlanilha.Text = "Importar";
            }
        }
    }
}
