using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiaqodbManager
{
    public partial class EditArray : Form
    {
        public EditArray()
        {
            InitializeComponent();
        }
        public void SetArrayValue(Array arr)
        {
            foreach (object obj in arr)
            {
                if (textBox1.Text == string.Empty)
                {
                    this.textBox1.AppendText(obj.ToString());
                }
                else
                {
                    this.textBox1.AppendText(Environment.NewLine + obj.ToString());
                }



            }
        }
        private Array values;
        public Array GetArrayValues()
        {
            return values;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != string.Empty)
            {
                try
                {
                    string[] arrayStr = textBox1.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    values = Array.CreateInstance(elementType, arrayStr.Length);
                    for (int i = 0; i < arrayStr.Length; i++)
                    {
                        values.SetValue(Convert.ChangeType(arrayStr[i], elementType), i);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else
            {
                values = Array.CreateInstance(elementType, 0);
            }
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        internal void SetArrayType(Type type)
        {
            this.elementType = type.GetElementType();
        }
        Type elementType;
    }
}
