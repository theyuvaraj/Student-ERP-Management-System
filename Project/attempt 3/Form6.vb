Imports MySql.Data.MySqlClient
Imports Mysqlx

Public Class Form6

    Private Sub DataGridView1_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub
    Dim connection As New MySqlConnection("Server=localhost;Database=semester;User Id=root;Password=Tiger123;")
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        ' Clear the DataGridView
        DataGridView1.Rows.Clear()

        ' Get the selected semester
        Dim selectedSemester As String = ComboBox1.SelectedItem.ToString()

        ' Query the database to get the subjects for the selected semester
        Dim query As String = "SELECT id, name, credits FROM subjects WHERE semester = @semester"
        Dim command As New MySqlCommand(query, connection)
        command.Parameters.AddWithValue("@semester", selectedSemester)

        connection.Open()
        Dim reader As MySqlDataReader = command.ExecuteReader()

        ' Populate the DataGridView with the subjects for the selected semester
        While reader.Read()
            Dim newRow As String() = {reader("id").ToString(), reader("name").ToString(), reader("credits").ToString()}
            DataGridView1.Rows.Add(newRow)
        End While

        reader.Close()
        connection.Close()
    End Sub

    Private Sub Update_Click(sender As Object, e As EventArgs) Handles Update_btn.Click
        connection.Open()

        ' Loop through each row in the DataGridView
        For Each row As DataGridViewRow In DataGridView1.Rows
            ' Update the existing subject
            Dim query As String = "UPDATE subjects SET name = @name, credits = @credits WHERE id = @id"
            Dim command As New MySqlCommand(query, connection)

            If row.Cells(0).Value IsNot Nothing AndAlso
               row.Cells(1).Value IsNot Nothing AndAlso
               row.Cells(2).Value IsNot Nothing Then
                command.Parameters.AddWithValue("@id", row.Cells(0).Value)
                command.Parameters.AddWithValue("@name", row.Cells(1).Value.ToString())
                command.Parameters.AddWithValue("@credits", row.Cells(2).Value)
                command.ExecuteNonQuery()
            End If
        Next

        connection.Close()
        MessageBox.Show("Subjects updated successfully!")
    End Sub

    Private Sub Form6_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Add columns to the DataGridView
        DataGridView1.ColumnCount = 3
        DataGridView1.Columns(0).Name = "ID"
        DataGridView1.Columns(1).Name = "Name"
        DataGridView1.Columns(2).Name = "Credits"
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            connection.Open()

            ' Get the values from the current row of the DataGridView
            Dim currentRow As DataGridViewRow = DataGridView1.CurrentRow
            Dim newId As Object = currentRow.Cells(0).Value
            Dim newName As Object = currentRow.Cells(1).Value
            Dim newCredits As Object = currentRow.Cells(2).Value

            ' Get the selected semester value from the combobox
            Dim selectedSemester As String = ComboBox1.SelectedItem.ToString()

            ' Check if the values are not null
            If newId IsNot Nothing AndAlso
           newName IsNot Nothing AndAlso
           newCredits IsNot Nothing AndAlso
           selectedSemester IsNot Nothing Then
                ' Insert a new subject into the database
                Dim query As String = "INSERT INTO subjects (id, name, credits, semester) VALUES (@id, @name, @credits, @semester)"
                Dim command As New MySqlCommand(query, connection)
                command.Parameters.AddWithValue("@id", newId)
                command.Parameters.AddWithValue("@name", newName.ToString())
                command.Parameters.AddWithValue("@credits", newCredits)
                command.Parameters.AddWithValue("@semester", selectedSemester)
                command.ExecuteNonQuery()
            End If

            connection.Close()
            MessageBox.Show("New subject added successfully!")

        Catch ex As Exception
            MessageBox.Show("Error adding new subject: " & ex.Message)
        End Try
    End Sub
End Class