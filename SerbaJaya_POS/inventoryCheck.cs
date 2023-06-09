﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerbaJaya_POS
{

    public partial class inventoryCheck : Form
    {
        public string selectedPO;
        public string status = "true";


        void loadPO(string filter = null)
        {
            string query =
                $"SELECT PurchaseOrderID, S.SupplierName, OrderDate " +
                $"FROM PurchaseOrder P " +
                $"INNER JOIN Supplier S ON S.SupplierID = P.SupplierID " +
                $"WHERE (( PurchaseOrderID IS NULL OR PurchaseOrderID LIKE '%{filter}%' ) OR " +
                    $"( S.SupplierName IS NULL or S.SupplierName LIKE '%{filter}%' ) OR " +
                    $"( OrderDate IS NULL or OrderDate LIKE '%{filter}%' )) AND " +
                    $"IsDone LIKE 'false' ";

            var conn = new Connection.Connection_Query();
            conn.OpenConnection();

            try
            {
                dgvPO.DataSource = conn.ShowDataInGridView(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi Error, harap hubungi bantuan: " + ex.ToString());
            }
            finally
            {
                conn.CloseConnectoin();
            }
        }


        void loadDetail(string id)
        {
            string query =
                "SELECT I.ItemID, I.ItemName, Quantity, Price, Note FROM PurchaseOrderDetail PO " +
                "INNER JOIN Dataitem I ON I.ItemID = PO.ItemID " +
                $"WHERE PurchaseOrderID = '{id}' AND IsComplete = 'false' ";

            MessageBox.Show(query);
            var conn = new Connection.Connection_Query();
            try
            {
                conn.OpenConnection();
                dgvPODetail.ReadOnly = false;
                dgvPODetail.DataSource = conn.ShowDataInGridView(query);
                dgvPODetail.Columns["Price"].Visible = false;
                dgvPODetail.Columns["Note"].ReadOnly = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                conn.CloseConnectoin();
            }
        }

        void updateItem()
        {

            foreach (DataGridViewRow row in dgvPODetail.Rows)
            {
                string complete;
                string itemNote = null;


                string id = row.Cells[1].Value.ToString();
                bool isSelected = Convert.ToBoolean(row.Cells[0].Value);
                if (isSelected)
                {
                    complete = "true";

                    int stock = Convert.ToInt32(row.Cells["Quantity"].Value);
                    int cost = Convert.ToInt32(row.Cells["Price"].Value);

                    string queryUpdt = $"UPDATE DataItem SET " +
                    $"Stock = Stock+{stock}, " +
                    $"Cost = (Cost*Stock+{cost * stock})/(Stock+{stock}) " +
                    $"WHERE ItemID = '{id}' ";

                    MessageBox.Show(queryUpdt);

                    var conn = new Connection.Connection_Query();
                    conn.OpenConnection();

                    try
                    {
                        conn.ExecuteQueires(queryUpdt);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        conn.CloseConnectoin();
                    }
                }
                else
                {
                    status = "false";
                    complete = "false";

                    string tempNote = row.Cells["Note"].Value.ToString();
                    itemNote=  tempNote == "" ? "Jumlah tidak sesuai" : tempNote.ToString();

                }

                updatePODetail(id, itemNote, complete);
            }
            updatePO();
        }

        void updatePO()
        {
            string queryUpdt = "UPDATE PurchaseOrder SET " +
                $"IsDone = '{status}' " +
                $"WHERE PurchaseOrderID = '{selectedPO}' ";
            MessageBox.Show(queryUpdt);
            var conn = new Connection.Connection_Query();
            conn.OpenConnection();

            try
            {
                conn.ExecuteQueires(queryUpdt);
                MessageBox.Show("Stock item berhasil diperbaharui.");
                formRefresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                conn.CloseConnectoin();
            }
        }

        void updatePODetail(string idItem, string note, string status)
        {
            var conn = new Connection.Connection_Query();
            conn.OpenConnection();

            try
            {
                string query =
                    "UPDATE PurchaseOrderDetail SET " +
                    $"Note = '{note}', " +
                    $"IsComplete = '{status}' " +
                    $"WHERE PurchaseOrderID = '{selectedPO}' AND " +
                    $"ItemID = '{idItem}' ";

                MessageBox.Show(query);
                conn.ExecuteQueires(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail to update detail: " + ex.ToString());
            }
            finally
            {
                conn.CloseConnectoin();
            }
        }

        public inventoryCheck()
        {
            InitializeComponent();
        }

        void formRefresh()
        {
            loadPO();
            dgvPODetail.DataSource = null;
            dgvPODetail.Rows.Clear();

            status = "true";
        }

        private void inventoryCheck_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            formRefresh();
        }

        private void dgvPO_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            var currentColumn = senderGrid.Columns[e.ColumnIndex];

            if (currentColumn is DataGridViewButtonColumn &&
               e.RowIndex >= 0)
            {
                selectedPO = dgvPO.Rows[e.RowIndex].Cells[1].Value.ToString();

                loadDetail(selectedPO);

            }
        }

        private void dgvPODetail_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            formRefresh();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            DialogResult confirmDisc = MessageBox.Show("Konfirmasi penerimaan barang?",
                   "Confirmation", MessageBoxButtons.YesNo);
            if (confirmDisc == DialogResult.Yes)
            {
                updateItem();
            }
        }
    }
}
