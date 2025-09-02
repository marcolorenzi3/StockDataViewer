CREATE DATABASE AlphaStockDatabase
GO
 
USE AlphaStockDatabase
GO
 
CREATE SCHEMA AlphaStockSchema
GO

CREATE TABLE AlphaStockSchema.TimeSeriesDaily (
    Date   DATE,
    Symbol VARCHAR(10),
    [Open]   DECIMAL(10,2),
    High   DECIMAL(10,2),
    Low    DECIMAL(10,2),
    [Close]  DECIMAL(10,2),
    Volume BIGINT,
    PRIMARY KEY (Date, Symbol)
);