namespace CinemaBooking

open System
open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

module Program =

    let colBg = Color.FromArgb(25, 25, 35)        
    let colSidebar = Color.FromArgb(32, 32, 42)   
    let colAccent = Color.FromArgb(100, 200, 255) 
    let colVipBorder = Color.FromArgb(255, 215, 0)
    let colVipFill = Color.Black                  
    let colSeatAvail = Color.FromArgb(50, 50, 65) 
    let colSeatBooked = Color.FromArgb(200, 60, 60) 
    let colSelected = Color.FromArgb(46, 204, 113)  
    let colDanger = Color.FromArgb(231, 76, 60)

    type SeatControl(seat: Seat, onClick: unit -> unit) as this =
        inherit Control()
        let mutable isHovered = false
        do
            this.DoubleBuffered <- true
            this.Size <- Size(50, 50)
            this.Cursor <- Cursors.Hand
            this.Margin <- Padding(6) 
            this.MouseEnter.Add(fun _ -> isHovered <- true; this.Invalidate())
            this.MouseLeave.Add(fun _ -> isHovered <- false; this.Invalidate())
            this.Click.Add(fun _ -> onClick())

        override this.OnPaint(e) =
            let g = e.Graphics
            g.SmoothingMode <- SmoothingMode.AntiAlias
            let backColor, borderColor = 
                match seat.Status with
                | Booked _ -> colSeatBooked, colSeatBooked
                | Selected -> colSelected, colSelected
                | Available -> 
                    if seat.Tier = VIP then 
                        if isHovered then (Color.FromArgb(40, 40, 40), colVipBorder) 
                        else (colVipFill, colVipBorder)
                    else 
                        if isHovered then (Color.FromArgb(70, 70, 90), colAccent)
                        else (colSeatAvail, Color.Gray)

            let rect = Rectangle(4, 8, this.Width - 8, this.Height - 12)
            use brush = new SolidBrush(backColor)
            g.FillRectangle(brush, rect)
            let borderThickness = if seat.Tier = VIP || seat.Status = Selected then 2.0f else 1.0f
            use pen = new Pen(borderColor, borderThickness)
            g.DrawRectangle(pen, rect)
            let armColor = if seat.Tier = VIP then Color.FromArgb(60, 60, 60) else Color.FromArgb(40,40,40)
            g.FillRectangle(new SolidBrush(armColor), 2, 15, 4, 15)
            g.FillRectangle(new SolidBrush(armColor), this.Width - 6, 15, 4, 15)
    
            let textColor = if seat.Status = Selected then Color.Black else Color.White
            let txt = sprintf "%d-%d" (seat.RowIndex + 1) (seat.ColIndex + 1)
            TextRenderer.DrawText(g, txt, this.Font, rect, textColor, TextFormatFlags.HorizontalCenter ||| TextFormatFlags.VerticalCenter)
            if seat.Tier = VIP then
                use f = new Font("Arial", 6.0f, FontStyle.Bold)
                let vipRect = Rectangle(0, 0, this.Width, 10)
                TextRenderer.DrawText(g, "VIP", f, vipRect, colVipFill, TextFormatFlags.HorizontalCenter)

    type CinemaForm() as form =
        inherit Form()

        let mutable currentState = CoreLogic.initializeCinema 5 5
        
        let mainSplit = new TableLayoutPanel(Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1)
        let leftPanel = new Panel(Dock = DockStyle.Fill, BackColor = colBg)
        let seatGrid = new TableLayoutPanel(AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent)
        let rightPanel = new Panel(Dock = DockStyle.Fill, BackColor = colSidebar, Padding = Padding(20))
        
        let lblScreen = new Label(Text = "___ SCREEN ___", ForeColor = Color.Gray, AutoSize = true, Font = new Font("Consolas", 10.0f))
        
        let btnReset = new Button(Text = "RESET SYSTEM", Dock = DockStyle.Top, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = colDanger, ForeColor = Color.White, Font = new Font("Segoe UI", 8.0f, FontStyle.Bold), Cursor = Cursors.Hand)
        
        let lblTitle = new Label(Text = "\nCINEMA\nBOOKING", Font = new Font("Segoe UI", 20.0f, FontStyle.Bold), ForeColor = colAccent, AutoSize = true, Dock = DockStyle.Top)
        let lblDetails = new Label(Text = "\n\nSelect a seat...", Font = new Font("Segoe UI", 11.0f), ForeColor = Color.LightGray, AutoSize = true, Dock = DockStyle.Top)
        let btnConfirm = new Button(Text = "CONFIRM", Dock = DockStyle.Bottom, Height = 50, FlatStyle = FlatStyle.Flat, BackColor = Color.Gray, ForeColor = Color.Black, Enabled = false, Font = new Font("Segoe UI", 12.0f, FontStyle.Bold))

        let centerContent () =
            if leftPanel.Width > 0 && leftPanel.Height > 0 then
                let x = (leftPanel.Width - seatGrid.Width) / 2
                let y = (leftPanel.Height - seatGrid.Height) / 2
                seatGrid.Location <- Point(x, y)
                lblScreen.Location <- Point((leftPanel.Width - lblScreen.Width) / 2, seatGrid.Top - 30)

        do
            form.Text <- "PL-3 (F# based) Cinema System"
            form.Size <- Size(900, 600)
            form.StartPosition <- FormStartPosition.CenterScreen
            
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75.0f)) |> ignore
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25.0f)) |> ignore
            
            form.Controls.Add(mainSplit)
            mainSplit.Controls.Add(leftPanel, 0, 0)
            mainSplit.Controls.Add(rightPanel, 1, 0)

            rightPanel.Controls.Add(lblDetails)
            rightPanel.Controls.Add(lblTitle)
            rightPanel.Controls.Add(btnReset)
            rightPanel.Controls.Add(btnConfirm)
            
            btnReset.FlatAppearance.BorderSize <- 0
            btnConfirm.FlatAppearance.BorderSize <- 0

            leftPanel.Controls.Add(seatGrid)
            leftPanel.Controls.Add(lblScreen)

            leftPanel.Resize.Add(fun _ -> centerContent())
            form.Shown.Add(fun _ -> centerContent())

            btnConfirm.Click.Add(fun _ -> 
                match CoreLogic.confirmBooking currentState with
                | Ok ticket -> 
                    MessageBox.Show(sprintf "Success!\nTicket: %s" ticket.TicketID) |> ignore
                    form.RenderSeats() 
                    form.UpdateSidebar(None)
                    centerContent() 
                | Error msg -> MessageBox.Show(msg) |> ignore
            )

            btnReset.Click.Add(fun _ -> 
                let response = MessageBox.Show("Are you sure you want to delete all bookings? This cannot be undone.", "Reset System", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                if response = DialogResult.Yes then
                    CoreLogic.resetCinema currentState |> ignore
                    form.RenderSeats()
                    form.UpdateSidebar(None)
                    centerContent()
                    MessageBox.Show("System Reset Complete.") |> ignore
            )

            form.RenderSeats()

        member this.RenderSeats() =
            seatGrid.Controls.Clear()
            seatGrid.RowCount <- currentState.Rows
            seatGrid.ColumnCount <- currentState.Cols
            for r in 0 .. currentState.Rows - 1 do
                for c in 0 .. currentState.Cols - 1 do
                    let seat = currentState.Seats.[r, c]
                    let ctrl = new SeatControl(seat, fun () -> this.HandleClick(r, c))
                    seatGrid.Controls.Add(ctrl, c, r)

        member this.HandleClick(r, c) =
            let result = CoreLogic.toggleSelection currentState r c
            match result with
            | Ok updatedSeat ->
                this.RenderSeats()
                this.UpdateSidebar(if updatedSeat.Status = Selected then Some updatedSeat else None)
            | Error _ -> ()

        member this.UpdateSidebar(seat: Seat option) =
            match seat with
            | Some s ->
                let typeStr = if s.Tier = VIP then "VIP (Premium)" else "Standard"
                lblDetails.Text <- sprintf "\n\nSEAT SELECTED\nRow: %d\nSeat: %d\n\nPrice: $%.2f\n%s" (s.RowIndex+1) (s.ColIndex+1) s.Price typeStr
                btnConfirm.Enabled <- true
                btnConfirm.BackColor <- colAccent
            | None ->
                lblDetails.Text <- "\n\nSelect a seat..."
                btnConfirm.Enabled <- false
                btnConfirm.BackColor <- Color.Gray

    [<EntryPoint>]
    [<STAThread>]
    let main argv =
        try

            let testReport = TestRunner.runAllTests()
            MessageBox.Show(testReport, "Unit Test Results") |> ignore

            Storage.initializeDatabase()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(false)
            Application.Run(new CinemaForm())
            0
        with
        | ex -> 
            MessageBox.Show("CRASH REPORT:\n" + ex.ToString(), "Startup Error") |> ignore
            1