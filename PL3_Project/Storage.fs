namespace CinemaBooking
open System
open System.IO
open Microsoft.Data.Sqlite

module Storage =
    let private connectionString = "Data Source=cinema.db"   
    let private executeNonQuery sql parameters =
        using (new SqliteConnection(connectionString)) (fun conn ->  
            conn.Open()
            using (new SqliteCommand(sql, conn)) (fun cmd ->
                for (name, value) in parameters do
                    cmd.Parameters.AddWithValue(name, value) |> ignore
                cmd.ExecuteNonQuery() |> ignore  

            )
        )

    let  initializeDatabase () =
        let sql = 
            """
            CREATE TABLE IF NOT EXISTS Tickets (
                TicketID TEXT PRIMARY KEY,
                RowIndex INTEGER,
                ColIndex INTEGER,
                Price DECIMAL,
                BookedAt TEXT
            );
            """
        executeNonQuery sql []

    initializeDatabase()

    let saveTicket (ticket: Ticket) =
        let sql = 
            """
            INSERT INTO Tickets (TicketID, RowIndex, ColIndex, Price, BookedAt) 
            VALUES ($id, $row, $col, $price, $date)
            """
        let parameters = [
            "$id", box ticket.TicketID
            "$row", box ticket.Row
            "$col", box ticket.Col
            "$price", box ticket.Price
            "$date", box (ticket.BookedAt.ToString("o"))
        ]
        executeNonQuery sql parameters

    let loadHistory () =
        let sql = "SELECT RowIndex, ColIndex FROM Tickets"
        let results = ResizeArray<int * int>()
        
        using (new SqliteConnection(connectionString)) (fun conn ->
            conn.Open()
            using (new SqliteCommand(sql, conn)) (fun cmd ->
                use reader = cmd.ExecuteReader()
                while reader.Read() do
                    let row = reader.GetInt32(0)
                    let col = reader.GetInt32(1)
                    results.Add((row, col))
            )
        )
        results.ToArray()

    let clearHistory () =
        let sql = "DELETE FROM Tickets"
        executeNonQuery sql []
        
