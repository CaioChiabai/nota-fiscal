using NotaFiscal.Controllers;

namespace NotaFiscal.Views
{
    public partial class FormImportador : Form
    {
        private readonly PlanilhaController _planilhaController;

        public FormImportador(PlanilhaController planilhaController)
        {
            InitializeComponent();
            _planilhaController = planilhaController;
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

                // Limpa o log para uma nova execução
                txtBoxLogs.Clear();

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
