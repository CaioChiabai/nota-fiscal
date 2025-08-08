namespace NotaFiscal.Views
{
    partial class FormImportador
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSelecionarPlanilha = new Button();
            btnImportarPlanilha = new Button();
            lblTituloImportacao = new Label();
            txtBoxCaminhoPlanilha = new TextBox();
            txtBoxLogs = new TextBox();
            SuspendLayout();
            // 
            // btnSelecionarPlanilha
            // 
            btnSelecionarPlanilha.Location = new Point(524, 113);
            btnSelecionarPlanilha.Name = "btnSelecionarPlanilha";
            btnSelecionarPlanilha.Size = new Size(28, 23);
            btnSelecionarPlanilha.TabIndex = 0;
            btnSelecionarPlanilha.Text = "...";
            btnSelecionarPlanilha.UseVisualStyleBackColor = true;
            btnSelecionarPlanilha.Click += btnSelecionarPlanilha_Click;
            // 
            // btnImportarPlanilha
            // 
            btnImportarPlanilha.Font = new Font("Arial", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnImportarPlanilha.Location = new Point(355, 165);
            btnImportarPlanilha.Name = "btnImportarPlanilha";
            btnImportarPlanilha.Size = new Size(82, 32);
            btnImportarPlanilha.TabIndex = 1;
            btnImportarPlanilha.Text = "Importar";
            btnImportarPlanilha.UseVisualStyleBackColor = true;
            btnImportarPlanilha.Click += btnImportarPlanilha_Click;
            // 
            // lblTituloImportacao
            // 
            lblTituloImportacao.AutoSize = true;
            lblTituloImportacao.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTituloImportacao.Location = new Point(224, 114);
            lblTituloImportacao.Name = "lblTituloImportacao";
            lblTituloImportacao.Size = new Size(294, 19);
            lblTituloImportacao.TabIndex = 2;
            lblTituloImportacao.Text = "Escolha a planilha que quer importar:";
            lblTituloImportacao.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtBoxCaminhoPlanilha
            // 
            txtBoxCaminhoPlanilha.Location = new Point(224, 136);
            txtBoxCaminhoPlanilha.Name = "txtBoxCaminhoPlanilha";
            txtBoxCaminhoPlanilha.Size = new Size(328, 23);
            txtBoxCaminhoPlanilha.TabIndex = 3;
            // 
            // txtBoxLogs
            // 
            txtBoxLogs.Location = new Point(12, 249);
            txtBoxLogs.Multiline = true;
            txtBoxLogs.Name = "txtBoxLogs";
            txtBoxLogs.ReadOnly = true;
            txtBoxLogs.ScrollBars = ScrollBars.Vertical;
            txtBoxLogs.Size = new Size(776, 189);
            txtBoxLogs.TabIndex = 4;
            txtBoxLogs.WordWrap = false;
            // 
            // FormImportador
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(800, 450);
            Controls.Add(txtBoxLogs);
            Controls.Add(txtBoxCaminhoPlanilha);
            Controls.Add(lblTituloImportacao);
            Controls.Add(btnImportarPlanilha);
            Controls.Add(btnSelecionarPlanilha);
            Name = "FormImportador";
            Text = "Importador de Planilha";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelecionarPlanilha;
        private Button btnImportarPlanilha;
        private Label lblTituloImportacao;
        private TextBox txtBoxCaminhoPlanilha;
        private TextBox txtBoxLogs;
    }
}