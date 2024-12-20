Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Diagnostics.Eventing
Imports System.Runtime.InteropServices
Imports MySql.Data.MySqlClient
Imports Mysqlx
Imports Excel = Microsoft.Office.Interop.Excel

Public Class Form3



    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Form1.Show()
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ' Get the selected table name from the combobox
        Dim tableName As String = ComboBox5.SelectedItem.ToString()

        ' Get the selected category from the combobox
        Dim category As String = ComboBox1.SelectedItem.ToString()

        ' Get the selected religion from the combobox
        Dim religion As String = ComboBox2.SelectedItem.ToString()

        ' Get the selected EWS and Minority options from the radiobuttons
        Dim ews As String = If(ewsr.Checked, "Yes", "No")
        Dim minority As String = If(RadioButton3.Checked, "Yes", "No")

        ' Create a SQL query to fetch the data
        Dim query As String = "SELECT " &
                          "COUNT(CASE WHEN gen = 'Male' THEN 1 ELSE NULL END) AS MaleCount, " &
                          "COUNT(CASE WHEN gen = 'Female' THEN 1 ELSE NULL END) AS FemaleCount, " &
                          "'" & category & "' AS Category, " &
                          "catg AS Category, " &  ' Use the catg column for Category
                          "relg AS Religion, " &  ' Use the relg column for Religion
                          "ews AS EWS, " &  ' Use the ews column for EWS
                          "minority AS Minority " &  ' Use the minority column for Minority
                          "FROM `" & tableName & "` " &
                          "WHERE catg = '" & category & "' AND "

        If religion = "All" Then
            query &= "relg IN ('Hindu', 'Christian', 'Muslim') AND "
        Else
            query &= "relg = '" & religion & "' AND "
        End If

        query &= "ews = '" & ews & "' AND minority = '" & minority & "'"

        ' Execute the query and get the results
        Dim dataTable As New DataTable()
        Dim connectionString As String = "Server=localhost;Database=project;User Id=root;Password=Tiger123;"
        Dim sqlConnection As New MySqlConnection(connectionString)
        Dim sqlCommand As New MySqlCommand(query, sqlConnection)

        Try
            sqlConnection.Open()
            Dim dataAdapter As New MySqlDataAdapter(sqlCommand)
            dataAdapter.Fill(dataTable)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        Finally
            sqlConnection.Close()
        End Try

        ' Add the data to the DataGridView
        If DataGridView1.DataSource IsNot Nothing Then
            Dim existingDataTable As DataTable = CType(DataGridView1.DataSource, DataTable)
            existingDataTable.Merge(dataTable)
        Else
            DataGridView1.DataSource = dataTable
        End If

        ' Set the column headers in the DataGridView

        DataGridView1.Columns("Religion").HeaderText = "Religion"
        DataGridView1.Columns("EWS").HeaderText = "EWS"
        DataGridView1.Columns("Minority").HeaderText = "Minority"
    End Sub

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim connectionString As String = "Server=localhost;Database=project;User Id=root;Password=Tiger123;"
        Dim connection As New MySqlConnection(connectionString)

        Try
            connection.Open()
        Catch ex As Exception
            MessageBox.Show("Error connecting to database: " & ex.Message)
            Return
        End Try

        Dim command As New MySqlCommand("SHOW TABLES", Connection)
        Dim tables As New DataTable()

        Try
            tables.Load(command.ExecuteReader())
        Catch ex As Exception
            MessageBox.Show("Error retrieving tables: " & ex.Message)
            Return
        End Try

        ComboBox5.Items.Clear()

        For Each row As DataRow In tables.Rows
            ComboBox5.Items.Add(row(0).ToString())
        Next

        connection.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' Create a new Excel application
        Dim excelApp As New Excel.Application
        Dim workbook As Excel.Workbook
        Dim worksheet As Excel.Worksheet

        ' Create a new workbook and worksheet
        workbook = excelApp.Workbooks.Add()
        worksheet = workbook.Sheets("Sheet1")

        ' Set the header row
        For i As Integer = 0 To DataGridView1.ColumnCount - 1
            worksheet.Cells(1, i + 1).Value = DataGridView1.Columns(i).HeaderText
        Next

        ' Export the data
        For i As Integer = 0 To DataGridView1.RowCount - 1
            For j As Integer = 0 To DataGridView1.ColumnCount - 1
                worksheet.Cells(i + 2, j + 1).Value = DataGridView1.Rows(i).Cells(j).Value
            Next
        Next

        ' Format the worksheet
        worksheet.Columns.AutoFit()
        worksheet.Rows.AutoFit()

        ' Save the workbook
        Dim saveFileDialog As New SaveFileDialog
        saveFileDialog.FileName = "DataGridViewExport"
        saveFileDialog.DefaultExt = "xlsx"
        saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx"
        If saveFileDialog.ShowDialog = DialogResult.OK Then
            workbook.SaveAs(saveFileDialog.FileName)
        End If

        ' Clean up
        workbook.Close()
        excelApp.Quit()
        Marshal.ReleaseComObject(workbook)
        Marshal.ReleaseComObject(excelApp)
    End Sub
End Class