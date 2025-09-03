if exists (select * from sysobjects where id = object_id('spx_FormataCDBListaAplicNoDia') and sysstat & 0xf = 4 )
    drop procedure spx_FormataCDBListaAplicNoDia
GO
 

CREATE PROCEDURE dbo.spx_FormataCDBListaAplicNoDia
 @pvchLista                       VARCHAR(MAX)        
,@pvchMsgOUT					  VARCHAR(MAX)	= ''		OUTPUT
AS
SET CONCAT_NULL_YIELDS_NULL ON	    
SET NOCOUNT ON
SET LOCK_TIMEOUT 10000
  
DECLARE  @intRetCode              INTEGER    

-- Conversão da string delimitada para JSON estruturado
IF @pvchLista <> ''
BEGIN
    DECLARE @jsonResult NVARCHAR(MAX) = '{"result":['
    
    -- Criar tabela temporária para processar os registros
    DECLARE @TempRegistros TABLE (
        Id INT IDENTITY(1,1),
        Registro NVARCHAR(500)
    )
    
    -- Separar registros por '!@'
    DECLARE @pos INT = 1
    DECLARE @nextPos INT
    DECLARE @registro NVARCHAR(500)
    
    WHILE @pos <= LEN(@pvchLista)
    BEGIN
        SET @nextPos = CHARINDEX('!@', @pvchLista, @pos)
        
        IF @nextPos = 0
            BREAK
            
        SET @registro = LTRIM(RTRIM(SUBSTRING(@pvchLista, @pos, @nextPos - @pos)))
        
        IF LEN(@registro) > 0
            INSERT INTO @TempRegistros (Registro) VALUES (@registro)
            
        SET @pos = @nextPos + 2
    END
    
    -- Processar cada registro e converter para JSON
    DECLARE @counter INT = 1
    DECLARE @totalRecords INT = (SELECT COUNT(*) FROM @TempRegistros)
    
    WHILE @counter <= @totalRecords
    BEGIN
        SELECT @registro = Registro FROM @TempRegistros WHERE Id = @counter
        
        -- Separar campos por '|'
        DECLARE @campos NVARCHAR(MAX) = @registro + '|' -- Adicionar | final para facilitar parsing
        DECLARE @campo1 NVARCHAR(50), @campo2 NVARCHAR(50), @campo3 NVARCHAR(100)
        DECLARE @campo4 NVARCHAR(50), @campo5 NVARCHAR(50), @campo6 NVARCHAR(50)
        DECLARE @campo7 NVARCHAR(50), @campo8 NVARCHAR(50)
        
        -- Extrair campos usando SUBSTRING e CHARINDEX
        SET @pos = 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo1 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        SET @pos = @nextPos + 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo2 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        SET @pos = @nextPos + 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo3 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        SET @pos = @nextPos + 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo4 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        SET @pos = @nextPos + 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo5 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        SET @pos = @nextPos + 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo6 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        SET @pos = @nextPos + 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo7 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        SET @pos = @nextPos + 1
        SET @nextPos = CHARINDEX('|', @campos, @pos)
        SET @campo8 = LTRIM(RTRIM(SUBSTRING(@campos, @pos, @nextPos - @pos)))
        
        -- Adicionar vírgula se não for o primeiro registro
        IF @counter > 1
            SET @jsonResult = @jsonResult + ','
        
        -- Montar objeto JSON
        SET @jsonResult = @jsonResult + '{'
        SET @jsonResult = @jsonResult + '"codcar":' + ISNULL(@campo1, '0') + ','
        SET @jsonResult = @jsonResult + '"numOpe":' + ISNULL(@campo2, '0') + ','
        SET @jsonResult = @jsonResult + '"idComPap":"' + ISNULL(REPLACE(@campo3, '"', '\"'), '') + '",'
        SET @jsonResult = @jsonResult + '"numest":' + ISNULL(@campo4, '0') + ','
        
        -- Converter valor monetário (ex: "20.000,00" -> 20000.00)
        DECLARE @valorFormatted NVARCHAR(20) = @campo5
        SET @valorFormatted = REPLACE(@valorFormatted, '.', '')  -- Remove separadores de milhar
        SET @valorFormatted = REPLACE(@valorFormatted, ',', '.') -- Converte vírgula decimal para ponto
        SET @jsonResult = @jsonResult + '"vrFinIda":' + ISNULL(@valorFormatted, '0') + ','
        
        -- Converter datas de DD/MM/YYYY para YYYY-MM-DD
        DECLARE @dtaplISO NVARCHAR(20) = NULL
        DECLARE @dtvctoISO NVARCHAR(20) = NULL
        
        -- Data aplicação
        IF @campo6 IS NOT NULL AND LEN(@campo6) = 10 AND @campo6 LIKE '__/__/____'
        BEGIN
            DECLARE @dia1 VARCHAR(2) = SUBSTRING(@campo6, 1, 2)
            DECLARE @mes1 VARCHAR(2) = SUBSTRING(@campo6, 4, 2)
            DECLARE @ano1 VARCHAR(4) = SUBSTRING(@campo6, 7, 4)
            SET @dtaplISO = @ano1 + '-' + @mes1 + '-' + @dia1
        END
        
        -- Data vencimento
        IF @campo7 IS NOT NULL AND LEN(@campo7) = 10 AND @campo7 LIKE '__/__/____'
        BEGIN
            DECLARE @dia2 VARCHAR(2) = SUBSTRING(@campo7, 1, 2)
            DECLARE @mes2 VARCHAR(2) = SUBSTRING(@campo7, 4, 2)
            DECLARE @ano2 VARCHAR(4) = SUBSTRING(@campo7, 7, 4)
            SET @dtvctoISO = @ano2 + '-' + @mes2 + '-' + @dia2
        END
        
        SET @jsonResult = @jsonResult + '"dtapl":' + CASE WHEN @dtaplISO IS NULL THEN 'null' ELSE '"' + @dtaplISO + '"' END + ','
        SET @jsonResult = @jsonResult + '"dtvcto":' + CASE WHEN @dtvctoISO IS NULL THEN 'null' ELSE '"' + @dtvctoISO + '"' END + ','
        SET @jsonResult = @jsonResult + '"nsuAut":' + ISNULL(@campo8, '0')
        SET @jsonResult = @jsonResult + '}'
        
        SET @counter = @counter + 1
    END
    
    SET @jsonResult = @jsonResult + ']}'
    SET @pvchMsgOUT = @jsonResult
END
ELSE
BEGIN
    -- Para outros tipos de lista ou quando vazio
    SET @pvchMsgOUT = CASE 
        WHEN @pvchLista = '' THEN '{"result":[]}'
        ELSE @pvchLista 
    END
END


   