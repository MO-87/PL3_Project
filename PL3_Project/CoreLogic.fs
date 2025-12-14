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


    let toggleSelection (state: CinemaState) (row: int) (col: int) =
        if row < 0 || row >= state.Rows || col < 0 || col >= state.Cols then
            Error "Invalid seat coordinates."
        else
            let currentSeat = state.Seats.[row, col]
            
            match currentSeat.Status with
            | Booked _ -> Error "This seat is already booked."
            | Selected ->
                state.Seats.[row, col] <- { currentSeat with Status = Available }
                Ok { currentSeat with Status = Available }
            | Available ->
                for r in 0 .. state.Rows - 1 do
                    for c in 0 .. state.Cols - 1 do
                        if state.Seats.[r, c].Status = Selected then
                            state.Seats.[r, c] <- { state.Seats.[r, c] with Status = Available }

                let newSeatState = { currentSeat with Status = Selected }
                state.Seats.[row, col] <- newSeatState
                Ok newSeatState
                
    let confirmBooking (state: CinemaState) =
        let mutable target : Seat option = None
        for r in 0 .. state.Rows - 1 do
            for c in 0 .. state.Cols - 1 do
                if state.Seats.[r, c].Status = Selected then
                    target <- Some state.Seats.[r, c]

        match target with
        | Some seat ->
            let ticket = {
                TicketID = generateTicketId(); Row = seat.RowIndex; Col = seat.ColIndex
                Price = seat.Price; BookedAt = DateTime.Now
            }
            state.Seats.[seat.RowIndex, seat.ColIndex] <- { seat with Status = Booked(ticket.TicketID) }
            Storage.saveTicket ticket
            Ok ticket
        | None -> Error "No seat selected."

