from os import path
from dataclasses import dataclass
import random
import names
import sqlite3
import argparse
import pathlib
import datetime

@dataclass
class Reservation:
    id: int         = -1
    type: str       = ""
    CustomerID: int = -1
    Status: str     = ""
    paid: int       = -1
    room: int       = -1
    start_date: str = ""
    end_date: str   = ""

def insert(con: sqlite3.Connection, reservation: Reservation):
    con.execute("INSERT INTO Reservations VALUES(?,?,?,?,?,?,?,?)", (reservation.id, reservation.type, reservation.CustomerID, reservation.Status, reservation.paid, reservation.room, reservation.start_date, reservation.end_date))


def main(db_path: pathlib):
    print("Going to edit DB: {}".format(str(db_path)))
    connection = sqlite3.connect(str(db_path))

    customer_count    = 0
    reservation_count = 0
    # See where the unique IDs are
    for row in connection.execute("SELECT * FROM sqlite_sequence"):
        if (row[0] == 'Customers'):
            customer_count = row[1]
        if (row[0] == 'Reservations'):
            reservation_count = row[1]

    if customer_count == 0:
        for i in range(0, 5):
            name = names.get_full_name().split()
            connection.execute("INSERT INTO Customers VALUES(?,?,?,?,?,?)", (i, "email", name[0], name[1], "000-000-0000", "00000000000000"))
            
        connection.commit()

    for row in connection.execute("SELECT * from Customers"):
        print(row)

    if reservation_count == 0:
        for i in range(0, 5):
            date               = datetime.datetime.now()
            new_res            = Reservation()
            new_res.id         = i 
            new_res.type       = "REGULAR"
            new_res.CustomerID = i
            new_res.Status     = "NOT_PAID"
            new_res.paid       = 0
            new_res.room       = 0
            new_res.start_date = str(date)
            new_res.end_date   = date + datetime.timedelta(days = random.randint(1,7))
            insert(connection, new_res)
    
        connection.commit()

    for row in connection.execute("SELECT * from Reservations"):
        print(row)
    
    connection.close()



if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Populate sqlite3 db. This ASSUMES the db has been created correctly.')
    #parser.add_argument('PATH', type=str, help="Location to the db file")

    args = parser.parse_args()

    program_Path = path.expandvars(r"%LOCALAPPDATA%\Packages\6531af47-2112-46b6-a9c4-468ad1553b49_6mt4vznfqn0e4\LocalState\Database.db")

    db_path = pathlib.Path(program_Path)

    if not db_path.exists():
        print("Can't find the file: {}".format(str(db_path)))

    if not db_path.is_dir and db_path.suffix == "db":
        print("This is not a db file. {}".format(str(db_path)))


    main(db_path)

