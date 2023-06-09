﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Connection.Connection_Query;

namespace SerbaJaya_POS
{
    public partial class AdminSupplier : Form
    {

        string idIncrement()
        {
            var conn = new Connection.Connection_Query();
            string query =
                "SELECT TOP 1 * FROM Supplier ORDER BY SupplierID DESC";

            int temp = 0;

            try
            {
                conn.OpenConnection();

                var dr = conn.DataReader(query);

                if (dr.Read())
                {
                    string tempID = dr.GetValue(0).ToString().Trim();
                    string lastDigit = tempID.Substring(tempID.Length - 3);
                    temp = Convert.ToInt32(lastDigit);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.CloseConnectoin();

            int digit = 3;
            int increment = temp + 1;

            var id = $"SS{increment.ToString().PadLeft(digit, '0')}";

            return id;
        }

        void insertData()
        {
            var conn = new Connection.Connection_Query();
            conn.OpenConnection();

            try
            {
                string query = "INSERT INTO Supplier " +
                    "(SupplierID, SupplierName, SupplierAddress, SupplierPhone, IsActive)" +
                    $"VALUES('{tbID.Text}', '{tbName.Text}', '{tbAddress.Text}', '{tbPhone.Text}', 'true')";

                conn.ExecuteQueires(query);

                MessageBox.Show("Data Successfully Inserted");
                refreshForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void updateData()
        {
            var conn = new Connection.Connection_Query();
            conn.OpenConnection();

            try
            {
                string query = "UPDATE Supplier " +
                    $"SET SupplierName = '{tbName.Text}', " +
                    $"SupplierAddress = '{tbAddress.Text}', " +
                    $"SupplierPhone = '{tbPhone.Text}' " +
                    $"WHERE SupplierID = '{tbID.Text}' ";

                conn.ExecuteQueires(query);
                MessageBox.Show("Data Berhasil di update!");
                refreshForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void handleStatus(string status, string ID)
        {

            var conn = new Connection.Connection_Query();

            conn.OpenConnection();
            try
            {
                string queryUpdtDisc = $"Update Supplier SET IsActive = '{status}' WHERE SupplierID = '{ID}' ";
                conn.ExecuteQueires(queryUpdtDisc);
                MessageBox.Show("status berhasil diperbarui!");

                refreshForm();
            }
            catch (Exception ex2)
            {
                MessageBox.Show("terjadi error pada database, harap hubungi pihak terkait.");
            }
            finally
            {
                conn.CloseConnectoin();
            }

        }

        void deleteData(String supplierID)
        {
            DialogResult result = MessageBox.Show("Hapus Supplier?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                var conn = new Connection.Connection_Query();
                conn.OpenConnection();

                try
                {
                    string query = $"DELETE FROM Supplier WHERE SupplierID = '{supplierID}' ";

                    conn.ExecuteQueires(query);
                    MessageBox.Show("Supplier berhasil dihapus!");
                    refreshForm();
                }
                catch (Exception ex)
                {
                    conn.CloseConnectoin();
                    DialogResult confirmDisc = MessageBox.Show("Data tidak dapat dihapus karena terikat dengan data lain,  Ingin non-aktifkan barang?",
                   "Confirmation", MessageBoxButtons.YesNo);
                    if (confirmDisc == DialogResult.Yes)
                    {
                        handleStatus("false", supplierID);
                    }
                }
            }
        }


        string selectString = "SupplierID, SupplierName, SupplierAddress, SupplierPhone";
        void loadData(string filter = null)
        {
            var conn = new Connection.Connection_Query();
            conn.OpenConnection();

            try
            {
                string query = $"SELECT {selectString} FROM Supplier WHERE " +
                      $"(( SupplierID IS NULL OR SupplierID LIKE '%{filter}%' ) OR " +
                    $"( SupplierName IS NULL or SupplierName LIKE '%{filter}%' ) OR " +
                    $"( SupplierAddress IS NULL or SupplierAddress LIKE '%{filter}%' ) OR " +
                    $"( SupplierPhone IS NULL or SupplierPhone LIKE '%{filter}%' )) AND " +
                    $"IsActive = 'true' ";

                dgvSupplier.DataSource = conn.ShowDataInGridView(query);
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

        void loadDataPast(string filter = null)
        {
            var conn = new Connection.Connection_Query();
            conn.OpenConnection();

            try
            {
                string query = $"SELECT {selectString} FROM Supplier WHERE " +
                      $"(( SupplierID IS NULL OR SupplierID LIKE '%{filter}%' ) OR " +
                    $"( SupplierName IS NULL or SupplierName LIKE '%{filter}%' ) OR " +
                    $"( SupplierAddress IS NULL or SupplierAddress LIKE '%{filter}%' ) OR " +
                    $"( SupplierPhone IS NULL or SupplierPhone LIKE '%{filter}%' )) AND " +
                    $"IsActive = 'false' ";

                dgvSupplierPast.DataSource = conn.ShowDataInGridView(query);
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

        public AdminSupplier()
        {
            InitializeComponent();
        }


        //checkForm
        bool checkFormEmpty()
        {
            if (tbName.Text != "" && tbAddress.Text != "" && tbPhone.Text != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        //clearForm
        void clearForm()
        {
            tbName.Clear();
            tbAddress.Clear();
            tbPhone.Clear();
        }

        //handle Button
        void handleButtonUpdate(bool status)
        {
            if (status == true)
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = true;
                btnCancel.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = false;
                btnCancel.Enabled = false;
            }
        }

        //handle load form
        void refreshForm()
        {
            clearForm();
            tbID.Text = idIncrement();
            handleButtonUpdate(false);
            loadData();
            loadDataPast();
        }

        private void AddSupplier_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            refreshForm();
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            loadData(tbFilter.Text);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (checkFormEmpty() == false)
            {
                insertData();
            }
            else
            {
                MessageBox.Show("Harap isi semua data terlebih dahulu!");
            }
        }

        private void dgvSupplier_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            var currentColumn = senderGrid.Columns[e.ColumnIndex];

            if (currentColumn is DataGridViewButtonColumn &&
               e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSupplier.Rows[e.RowIndex];

                string id = row.Cells[2].Value.ToString();
                string nama = row.Cells[3].Value.ToString();
                string alamat = row.Cells[4].Value.ToString();
                string phone = row.Cells[5].Value.ToString();


                if (currentColumn.HeaderText == "Delete")
                {
                    deleteData(id);
                }
                else if (currentColumn.HeaderText == "Edit")
                {

                    tbID.Text = id;
                    tbName.Text = nama;
                    tbAddress.Text = alamat;
                    tbPhone.Text = phone;
                    handleButtonUpdate(true);

                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            refreshForm();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (checkFormEmpty() == false)
            {
                updateData();
            }
            else
            {
                MessageBox.Show("harap isi semua data terlebih dahulu.");
            }
        }

        private void dgvSupplierPast_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            var currentColumn = senderGrid.Columns[e.ColumnIndex];

            if (currentColumn is DataGridViewButtonColumn &&
               e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSupplierPast.Rows[e.RowIndex];
                string id = row.Cells[1].Value.ToString();

                handleStatus("true", id);
                refreshForm();

            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
