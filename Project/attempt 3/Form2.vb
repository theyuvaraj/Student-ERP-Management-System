Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Windows.Forms
Public Class Form2
    Private lblScrollingText As Label
    Private tmrScrollingText As Timer
    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Form1.Show()
    End Sub

    Dim connectionString As String = "Server=localhost;Database=project;User Id=root;Password=Tiger123;"
    Dim connection As New MySqlConnection(connectionString)
    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            connection.Open()
        Catch ex As Exception
            MessageBox.Show("Error connecting to database: " & ex.Message)
            Return
        End Try

        Dim command As New MySqlCommand("SHOW TABLES", connection)
        Dim tables As New DataTable()

        Try
            tables.Load(command.ExecuteReader())
        Catch ex As Exception
            MessageBox.Show("Error retrieving tables: " & ex.Message)
            Return
        End Try

        ComboBox1.Items.Clear()

        For Each row As DataRow In tables.Rows
            ComboBox1.Items.Add(row(0).ToString())
        Next

        connection.Close()
        ' Create a Label control to display the scrolling text
        lblScrollingText = New Label()
        lblScrollingText.Text = "Please Refresh before Adding new record"
        lblScrollingText.AutoSize = True
        lblScrollingText.Font = New Font("Arial", 10)
        lblScrollingText.ForeColor = Color.Red
        lblScrollingText.BackColor = Color.Transparent
        lblScrollingText.Location = New Point(10, 10)

        ' Add the Label control to Panel4
        Panel4.Controls.Add(lblScrollingText)

        ' Create a Timer control to animate the scrolling text
        tmrScrollingText = New Timer()
        tmrScrollingText.Interval = 4 ' adjust the interval to control the scrolling speed
        AddHandler tmrScrollingText.Tick, AddressOf tmrScrollingText_Tick
        tmrScrollingText.Start()
    End Sub
    Private Sub tmrScrollingText_Tick(sender As Object, e As EventArgs)
        If lblScrollingText IsNot Nothing Then
            ' Get the current location of the Label control
            Dim x As Integer = lblScrollingText.Location.X
            Dim y As Integer = lblScrollingText.Location.Y

            ' Move the Label control to the left
            x -= 1

            ' If the Label control reaches the left edge, reset its position
            If x < -lblScrollingText.Width Then
                x = Panel4.Width
                ' Increment a counter to keep track of the number of iterations
                Static iterationCount As Integer = 0
                iterationCount += 1

                ' If the label has scrolled twice, stop the timer
                If iterationCount >= 2 Then
                    tmrScrollingText.Stop()
                End If
            End If

            ' Update the Label control's location
            lblScrollingText.Location = New Point(x, y)
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        connection.Open()

        ' Get the selected table name from the ComboBox
        Dim tableName As String = ComboBox1.SelectedItem.ToString()

        For Each row As DataGridViewRow In DataGridView1.Rows
            If Not row.IsNewRow Then
                Dim query As String = "INSERT INTO `" & tableName & "` (registrationnumber, name, fname, gen, catg, relg, ews, minority) " &
                                      "VALUES (@registrationnumber, @name, @fname, @gen, @catg, @relg, @ews, @minority)"

                Dim command As New MySqlCommand(query, connection)

                command.Parameters.AddWithValue("@registrationnumber", row.Cells(0).Value)
                command.Parameters.AddWithValue("@name", row.Cells(1).Value)
                command.Parameters.AddWithValue("@fname", row.Cells(2).Value)
                command.Parameters.AddWithValue("@gen", row.Cells(3).Value)
                command.Parameters.AddWithValue("@catg", row.Cells(4).Value)
                command.Parameters.AddWithValue("@relg", row.Cells(5).Value)
                command.Parameters.AddWithValue("@ews", row.Cells(6).Value)
                command.Parameters.AddWithValue("@minority", row.Cells(7).Value)

                command.ExecuteNonQuery()


            End If
        Next
        MessageBox.Show("Student Data is Inserted Successfully")


        ' Insert data into semester_1 table (Database 2)
        Dim connectionString2 As String = "Server=localhost;Database=semester;User Id=root;Password=Tiger123;"
        Dim query2 As String = "INSERT INTO students (batch, registration_number, name) VALUES (@batch, @registration_number, @name)"

        Using connection2 As New MySqlConnection(connectionString2)
            connection2.Open()
            Dim command2 As New MySqlCommand(query2, connection2)
            command2.Parameters.AddWithValue("@batch", ComboBox1.SelectedItem.ToString())
            command2.Parameters.AddWithValue("@registration_number", DataGridView1.Rows(0).Cells(0).Value)
            command2.Parameters.AddWithValue("@name", DataGridView1.Rows(0).Cells(1).Value)

            command2.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        If connection.State <> ConnectionState.Open Then
            connection.Open()
        End If

        ' Get the selected table name from the ComboBox
        Dim tableName As String = ComboBox1.SelectedItem.ToString()

        ' Create a query to retrieve all data from the selected table
        Dim query As String = "SELECT * FROM `" & tableName & "`"

        Dim command As New MySqlCommand(query, connection)
        Dim reader As MySqlDataReader = command.ExecuteReader()

        ' Clear the DataGridView before filling it with new data
        DataGridView1.Rows.Clear()

        ' Fill the DataGridView with the query results
        While reader.Read()
            DataGridView1.Rows.Add(reader("registrationnumber"), reader("name"), reader("fname"), reader("gen"), reader("catg"), reader("relg"), reader("ews"), reader("minority"))
        End While

        reader.Close()
        If connection.State = ConnectionState.Open Then
            connection.Close()
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        DataGridView1.Rows.Clear()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        connection.Open()

        ' Get the selected table name from the ComboBox
        Dim tableName As String = ComboBox1.SelectedItem.ToString()

        ' Get the registration number of the selected row
        Dim columnIndex As Integer = 0 ' Assuming the registration number is in the first column
        Dim registrationNumber As String = DataGridView1.SelectedRows(0).Cells(columnIndex).Value.ToString()

        ' Create a query to delete the selected row from the table
        Dim query As String = "DELETE FROM `" & tableName & "` WHERE registrationnumber = @registrationnumber"

        Dim command As New MySqlCommand(query, connection)

        ' Add a parameter for the registration number
        command.Parameters.AddWithValue("@registrationnumber", registrationNumber)

        ' Execute the delete query
        command.ExecuteNonQuery()
        Dim semesterConnection As New MySqlConnection("Server=localhost;Database=semester;User Id=root;Password=Tiger123;")
        semesterConnection.Open()

        Dim deleteStudentQuery As String = "DELETE FROM students WHERE registration_number = @registrationnumber"
        Dim deleteStudentCommand As New MySqlCommand(deleteStudentQuery, semesterConnection)

        deleteStudentCommand.Parameters.AddWithValue("@registrationnumber", registrationNumber)

        deleteStudentCommand.ExecuteNonQuery()

        semesterConnection.Close()

        connection.Close()
        MessageBox.Show("Selected Record is Deleted Successfully")
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click

        ' Get the selected table name from the ComboBox
        Dim tableName As String = ComboBox1.SelectedItem.ToString()

        ' Create a connection to the MySQL database

        connection.Open()


        ' Loop through each row in the DataGridView
        For Each row As DataGridViewRow In DataGridView1.Rows
            ' Create a command to update the table
            Dim command As New MySqlCommand("UPDATE `" & tableName & "` SET ", connection)

            ' Initialize a list to store the column names and values
            Dim columnNames As New List(Of String)
            Dim values As New List(Of String)

            ' Loop through each cell in the row
            For Each cell As DataGridViewCell In row.Cells
                ' Get the column name from the DataGridViewColumn
                Dim dgvColumnName As String = DataGridView1.Columns(cell.ColumnIndex).HeaderText

                ' Get the corresponding column name in the MySQL table
                Dim mysqlColumnName As String = GetMysqlColumnName(dgvColumnName)

                ' Add the column name and value to the lists
                columnNames.Add(mysqlColumnName)
                values.Add("@" & mysqlColumnName)
                If mysqlColumnName IsNot Nothing Then
                    If cell.Value IsNot Nothing Then
                        command.Parameters.AddWithValue("@" & mysqlColumnName, cell.Value.ToString())
                    Else
                        command.Parameters.AddWithValue("@" & mysqlColumnName, DBNull.Value)
                    End If
                Else
                    ' Handle the case where mysqlColumnName is null
                    ' You could throw an exception or log an error
                    Throw New Exception("mysqlColumnName is null for column " & dgvColumnName)
                End If
            Next

            ' Concatenate the column names and values into the UPDATE statement
            For i As Integer = 0 To columnNames.Count - 1
                command.CommandText &= columnNames(i) & " = " & values(i) & ", "
            Next

            ' Remove the trailing comma and space
            command.CommandText = command.CommandText.TrimEnd(" ,".ToCharArray())

            ' Add the WHERE clause to update the correct row
            command.CommandText &= " WHERE registrationnumber = @registrationnumber"
            If command.Parameters.IndexOf("@registrationnumber") = -1 Then
                command.Parameters.AddWithValue("@registrationnumber", row.Cells("registrationnumber").Value.ToString())
            End If

            ' Execute the update command
            command.ExecuteNonQuery()
        Next
        MessageBox.Show("Updated SUCCESSFULLY")
        ' Close the connection
        connection.Close()
    End Sub
    Private Function GetMysqlColumnName(dgvColumnName As String) As String
        Select Case dgvColumnName
            Case "Registration Number"
                Return "registrationnumber"
            Case "Name"
                Return "name"
            Case "Father Name"
                Return "fname"
            Case "Gender"
                Return "gen"
            Case "Category"
                Return "catg"
            Case "Religion"
                Return "relg"
            Case "EWS"
                Return "ews"
            Case "Minority"
                Return "minority"
                ' Add more cases for other columns
            Case Else
                Throw New Exception("Unknown column name: " & dgvColumnName)
        End Select
    End Function


End Class