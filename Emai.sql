CREATE FUNCTION dbo.IsValidEmail(@Email NVARCHAR(255))
RETURNS BIT
AS
BEGIN
    IF @Email IS NULL RETURN 0;
    
    -- Basic structure + allowed chars + length
    IF @Email NOT LIKE '%_@__%.__%' 
       OR @Email LIKE '%@%@%' 
       OR PATINDEX('%[^a-zA-Z0-9.@_-]%', @Email) <> 0
       OR LEN(@Email) > 254 
       OR LEN(@Email) < 6
       RETURN 0;

    -- No consecutive dots
    IF @Email LIKE '%.@%' OR @Email LIKE '@.%' OR @Email LIKE '..%'
       RETURN 0;

    -- No dot at the very end
    IF RIGHT(@Email, 1) = '.' 
       RETURN 0;

    RETURN 1;
END;
GO


-- Usage in table
CREATE TABLE dbo.Customers
(
    ...
    Email NVARCHAR(255) NULL,
    
    CONSTRAINT CHK_Customers_Email 
    CHECK (dbo.IsValidEmail(Email) = 1 OR Email IS NULL)
);
