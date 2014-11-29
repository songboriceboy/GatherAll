using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BlogGather;

namespace GatherAll
{
    public partial class Form1 : Form
    {
        private DataTable m_ataTable;
        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.RunWorkerAsync();
            m_ataTable = new DataTable();

            m_ataTable.Columns.Add("标题", System.Type.GetType("System.String"));
            m_ataTable.Columns.Add("内容", System.Type.GetType("System.String"));


  

            this.dataGridView1.DataSource = m_ataTable;
            this.dataGridView1.Columns[1].Visible = false;
            this.dataGridView1.Columns[0].Width = this.Width;
        }
        private void AddBlog(BlogGather.BlogGatherCnblogs.DelegatePara dp)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new BlogGatherCnblogs.GreetingDelegate(this.AddBlog), dp);
                return;
            }
            DataRow row1 = m_ataTable.NewRow();
            row1["标题"] = dp.strTitle;
            row1["内容"] = dp.strContent;
            m_ataTable.Rows.Add(row1);
            this.dataGridView1.DataSource = m_ataTable;
            this.dataGridView1.Columns[1].Visible = false;
            this.dataGridView1.Columns[0].Width = this.Width;
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BlogGatherCnblogs bgb = new BlogGatherCnblogs(this.toolStripTextBox1.Text);
            bgb.delAddBlog += new BlogGatherCnblogs.GreetingDelegate(this.AddBlog);
            bgb.GatherBlog(e);
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (this.dataGridView1.RowCount <= 0)
                return;

            if (this.dataGridView1.CurrentCell == null)
                return;
            string strContent = this.dataGridView1.Rows[this.dataGridView1.CurrentCell.RowIndex].Cells[1].Value.ToString();
            this.webBrowser1.DocumentText = strContent;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("全部博客下载完成!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.toolStripTextBox1.Text = "ice-river";
        }


    }
}
