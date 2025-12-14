namespace CinemaBooking

open System
open System.Text

module TestRunner =

    let private runTestCase (sb: StringBuilder) (testName: string) (testFunc: unit -> bool) =
        try
            if testFunc() then
                sb.AppendLine(sprintf "[PASS] %s" testName) |> ignore
            else
                sb.AppendLine(sprintf "[FAIL] %s" testName) |> ignore
        with
        | ex -> sb.AppendLine(sprintf "[FAIL] %s (Exception: %s)" testName ex.Message) |> ignore

    let runAllTests () =
        let sb = StringBuilder()
        sb.AppendLine("=== AUTOMATED TEST REPORT ===") |> ignore
        sb.AppendLine() |> ignore

        Storage.clearHistory() 

        let state = CoreLogic.initializeCinema 5 5
        for r in 0 .. 4 do
            for c in 0 .. 4 do
                state.Seats.[r, c] <- { state.Seats.[r, c] with Status = Available }


        runTestCase sb "Bounds Check (Row 99)" (fun () ->
            match CoreLogic.toggleSelection state 99 0 with 
            | Error _ -> true 
            | Ok _ -> false
        )

        runTestCase sb "Select Seat (0,0)" (fun () ->
            match CoreLogic.toggleSelection state 0 0 with
            | Ok seat -> seat.Status = Selected
            | Error _ -> false
        )

        runTestCase sb "Single Selection Rule" (fun () ->
            let _ = CoreLogic.toggleSelection state 0 1
            let previousSeat = state.Seats.[0,0]
            previousSeat.Status = Available 
        )

        runTestCase sb "Confirm Booking generates Ticket" (fun () ->
            match CoreLogic.confirmBooking state with
            | Ok ticket -> ticket.Row = 0 && ticket.Col = 1
            | Error _ -> false
        )

        runTestCase sb "Prevent Selecting Booked Seat" (fun () ->
            match CoreLogic.toggleSelection state 0 1 with
            | Error msg -> msg.Contains("booked")
            | Ok _ -> false
        )

        runTestCase sb "VIP Tier Assignment (Row 4)" (fun () ->
            let vipSeat = state.Seats.[4, 4] 
            vipSeat.Tier = VIP
        )

        runTestCase sb "VIP Pricing Check ($18.00)" (fun () ->
            let vipSeat = state.Seats.[4, 4]
            vipSeat.Price = 18.00m
        )

        runTestCase sb "Standard Seats are Free (Expect Fail)" (fun () ->
            let stdSeat = state.Seats.[0, 0]
            stdSeat.Price = 0.00m 
        )

        Storage.clearHistory() 
        sb.AppendLine("\n[INFO] Test Database Cleared.") |> ignore

        sb.ToString()