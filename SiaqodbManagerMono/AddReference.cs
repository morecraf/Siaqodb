using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Sqo;

namespace SiaqodbManager
{
	public partial class AddReference : Form
	{
		public AddReference()
		{
			InitializeComponent();
		}
		private List<ReferenceItem> assemblies = new List<ReferenceItem>();
		private List<NamespaceItem> namespaces = new List<NamespaceItem>();
		
		private void btnOK_Click(object sender, EventArgs e)
		{
			if(Directory.Exists(Application.StartupPath ))
			{
				assemblies.Clear();
				namespaces.Clear();
				Sqo.Siaqodb siaqodb = new Sqo.Siaqodb(Application.StartupPath );
                try
                {
                    siaqodb.DropType<ReferenceItem>();
                    siaqodb.DropType<NamespaceItem>();
                    foreach (object o in listBox1.Items)
                    {
                        ReferenceItem refItem = o as ReferenceItem;
                        if (refItem == null)
                        {
                            refItem = new ReferenceItem(o.ToString());
                        }
                        assemblies.Add(refItem);
                        siaqodb.StoreObject(refItem);

                        if (File.Exists(refItem.Item))
                        {
                            try
                            {
                                File.Copy(refItem.Item, Application.StartupPath + Path.DirectorySeparatorChar + Path.GetFileName(refItem.Item), true);
                                

                            }
                            catch
                            {

                            }
                        }

                    }
                    foreach (string s in textBox1.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        NamespaceItem nobj = new NamespaceItem(s);
                        namespaces.Add(nobj);
                        siaqodb.StoreObject(nobj);
                    }
                }
                finally
                {
                    siaqodb.Close();
                }
			}
			
			this.DialogResult = DialogResult.OK;
		}
		public List<ReferenceItem> GetReferences()
		{
			return assemblies;
		}
		public List<NamespaceItem> GetNamespaces()
		{
			return namespaces;
		}
		private void btnCancel_Click(object sender, EventArgs e)
		{
            
            this.Close();
		}

		private void btnAddReference_Click(object sender, EventArgs e)
		{
			OpenFileDialog opf = new OpenFileDialog();
			opf.Filter = "assembly files (*.dll;*.exe)|*.dll;*.exe";
			opf.InitialDirectory = Application.StartupPath;
			opf.Multiselect = false;
			if (opf.ShowDialog() == DialogResult.OK)
			{
				listBox1.Items.Add(opf.FileName);
			}
		}

		private void btnRemoveReference_Click(object sender, EventArgs e)
		{
			if (this.listBox1.SelectedItem != null)
			{
				this.listBox1.Items.Remove(this.listBox1.SelectedItem);
			}
		}

		private void AddReference_Load(object sender, EventArgs e)
		{
            if (Directory.Exists(Application.StartupPath ))
            {
                Sqo.Siaqodb siaqodb = new Sqo.Siaqodb(Application.StartupPath );
                try
                {
                    Sqo.IObjectList<ReferenceItem> references = siaqodb.LoadAll<ReferenceItem>();
                    foreach (ReferenceItem refItem in references)
                    {
                        listBox1.Items.Add(refItem);
                    }
                    Sqo.IObjectList<NamespaceItem> namespacesItems = siaqodb.LoadAll<NamespaceItem>();
                    foreach (NamespaceItem nItem in namespacesItems)
                    {
                        textBox1.Text += nItem + Environment.NewLine;
                    }
                }
                finally
                {
                    siaqodb.Close();
                }

            }
		}

        private void btnAddDefault_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add("System.dll");
            listBox1.Items.Add("System.Core.dll");
            listBox1.Items.Add("System.Windows.Forms.dll");
            listBox1.Items.Add("siaqodb.dll");
        }
	}
    [System.Reflection.Obfuscation(Exclude = true)]
	public class ReferenceItem : SqoDataObject
	{
		public ReferenceItem()
		{

		}
		public ReferenceItem(string item)
		{
			this.Item = item;
		}
		[Sqo.Attributes.MaxLength(2000)]
		public string Item;
		public override string ToString()
		{
			return Item;
		}
	}
    [System.Reflection.Obfuscation(Exclude = true)]
	public class NamespaceItem : Sqo.SqoDataObject
	{
		public NamespaceItem()
		{

		}
		public NamespaceItem(string item)
		{
			this.Item = item;
		}
		[Sqo.Attributes.MaxLength(2000)]
		public string Item;
		public override string ToString()
		{
			return Item;
		}
	}
}
