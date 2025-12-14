namespace CinemaBooking

open System

module CoreLogic =
    let initializeCinema rows cols : CinemaState =
        let vipStartRow = rows - 2 
        let seats = Array2D.init rows cols (fun r c -> 
            let isVip = r >= vipStartRow
            { 
                RowIndex = r; ColIndex = c; Status = Available
                Tier = if isVip then VIP else Standard
                Price = if isVip then 18.00m else 10.00m 
            }
        )
        let history = Storage.loadHistory()
        for (r, c) in history do
            if r < rows && c < cols && r >= 0 then
                seats.[r, c] <- { seats.[r, c] with Status = Booked("LOADED") }

        { Seats = seats; Rows = rows; Cols = cols }
