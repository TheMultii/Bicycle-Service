CREATE TABLE IF NOT EXISTS Users (
    uid          TEXT PRIMARY KEY
                      UNIQUE
                      NOT NULL,
    name         TEXT,
    surname      TEXT,
    login        TEXT UNIQUE
                      NOT NULL,
    password     TEXT NOT NULL,
	passwordSalt TEXT NOT NULL,
    account_type TEXT NOT NULL
                      CONSTRAINT account_type_check CHECK (account_type IN ('Customer', 'Service', 'Shop') ) 
);

CREATE TABLE IF NOT EXISTS Bicycles (
    uid   TEXT    PRIMARY KEY
                  UNIQUE
                  NOT NULL,
    owner TEXT    REFERENCES Users (uid) ON UPDATE CASCADE,
    brand TEXT    NOT NULL,
    model TEXT    NOT NULL,
    type  TEXT    CONSTRAINT bicycle_type_check CHECK (type IN ('road', 'mountain', 'hybrid') ) 
                  DEFAULT ('road'),
    price NUMERIC DEFAULT (399.0) 
);

CREATE TABLE IF NOT EXISTS OrderStatuses (
    uid        TEXT PRIMARY KEY
                    UNIQUE
                    NOT NULL,
    bicycle    TEXT REFERENCES Bicycles (uid) ON UPDATE CASCADE
                    NOT NULL,
    changed_by TEXT REFERENCES Users (uid) ON UPDATE CASCADE
                    NOT NULL,
    status     TEXT NOT NULL
);


