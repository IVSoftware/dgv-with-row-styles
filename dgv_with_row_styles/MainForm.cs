using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dgv_with_row_styles
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dataGridView.AllowUserToAddRows = false;

            // Set the DataSource of DGV to a binding list of 'DgvItem' instances.
            dataGridView.DataSource = DataSource;
            for (int i = 1; i <= 5; i++)
            {
                DataSource.Add(new DgvItem { Description = $"Item {i}" });
            }

            // Do a little column formatting
            dataGridView.Columns[nameof(DgvItem.IsChecked)].Width = 50;
            dataGridView.Columns[nameof(DgvItem.IsChecked)].HeaderText = string.Empty;
            dataGridView.Columns[nameof(DgvItem.Description)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Ensure the row isn't stuck in edit mode when a checkbox changes
            dataGridView.CurrentCellDirtyStateChanged += (sender, e) =>
            {
                if(dataGridView.CurrentCell is DataGridViewCheckBoxCell)
                {
                    dataGridView.EndEdit();
                }
            };

            // Get notified when a checkbox changes
            DataSource.ListChanged += (sender, e) =>
            {
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemChanged:
                        // Make changes to row styles when that happens.
                        refreshStyles();
                        break;
                }
            };

            // Init the styles
            refreshStyles();
        }

        private void refreshStyles()
        {
            for (int i = 0; i < DataSource.Count; i++)
            {
                var item = DataSource[i];
                var row = dataGridView.Rows[i];
                // Get all of the row cells that aren't checkboxes.
                var cells = row.Cells.Cast<DataGridViewCell>().Where(_ => !(_ is DataGridViewCheckBoxCell));
                if(item.IsChecked)
                {
                    row.DefaultCellStyle.BackColor = Color.Azure;
                    foreach (var cell in cells) cell.ReadOnly = false;
                }
                else
                {
                    dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(0xf0, 0xf0, 0xf0);
                    foreach (var cell in cells) cell.ReadOnly = true;
                }
            }
        }
        BindingList<DgvItem> DataSource { get; } = new BindingList<DgvItem>();
    }
    class DgvItem : INotifyPropertyChanged
    {
        bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (!Equals(_IsChecked, value))
                {
                    _IsChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
