namespace NotaFiscal.Views
{
    public partial class FormImportador : Form
    {
        public FormImportador()
        {
            InitializeComponent();
        }

        private void btnSelecionarPlanilha_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Planilhas Excel (*.xlsx)|*.xlsx";
            if (dialog.ShowDialog() == DialogResult.OK)
                txtBoxCaminhoPlanilha.Text = dialog.FileName;
        }

        private void btnImportarPlanilha_Click(object sender, EventArgs e)
        {

        }
    }
}
