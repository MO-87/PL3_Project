namespace CinemaBooking

type Ticket = {
    TicketID: string
    Row: int
    Col: int
    Price: decimal
    BookedAt: System.DateTime
}

type SeatTier = 
    | Standard 
    | VIP


type SeatStatus =
    | Available
    | Selected
    | Booked of ticketId: string

type Seat = {
    RowIndex: int
    ColIndex: int
    Status: SeatStatus
    Tier: SeatTier
    Price: decimal
}

type CinemaState = {
    Seats: Seat[,]
    Rows: int
    Cols: int
}