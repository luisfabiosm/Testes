if exists (select * from sysobjects where id = object_id('sps_CDBSolicitaLista') and sysstat & 0xf = 4 )
    drop procedure sps_CDBSolicitaLista
GO
 
 
/*
DECLARE @tinStatus      TINYINT
       ,@vchMsgErro     VARCHAR(8000)
       ,@intNSU         INTEGER
	   ,@vchMsgIN		VARCHAR(MAX)	
	   ,@vchMsgOUT		VARCHAR(MAX)
 
 SET @vchMsgIN = '{"produto":1, "agencia":157, "conta":8668574, "valor":20, "qtdParcelas":3}}'
 
 Exec sps_CDBSolicitaLista  
         @pvchOperador = ''
        ,@ptinCanal  = 0 
        ,@psmlAgencia  = 20
        ,@ptinPosto = 0
        ,@pvchEstacao = ''                 
        ,@ptinAcao = 7    
		,@pvchMsgIN		=  @vchMsgIN
        ,@pvchMsgOUT	 = @vchMsgOUT	 OUTPUT	
        ,@pintNSU        = @intNSU       OUTPUT   
        ,@ptinStatus     = @tinStatus    OUTPUT
        ,@pvchMsgErro    = @vchMsgErro  OUTPUT
 
PRINT @tinStatus
PRINT @vchMsgErro
PRINT @vchMsgOUT
 
--SELECT * FROM eventotransacao
-- WHERE datContabil = CONVERT(VARCHAR, GETDATE(), 112)
--   AND smlAgencia = 0
--   AND tinPOsto   = 1
--   AND intNSU = @intNSU
 
select TOP 10 * from fepocorrencia
 ORDER BY datIni DESC
*/
 
CREATE PROCEDURE dbo.sps_CDBSolicitaLista
 @pvchOperador                    VARCHAR(08)        --Operador
,@ptinCanal                       TINYINT            --Canal
,@psmlAgencia                     SMALLINT      = 0
,@ptinPosto                       TINYINT       = 1
,@pvchEstacao                     VARCHAR(15)   = ''
,@ptinAcao                        TINYINT  
--PARAMETROS VARIAVEIS
,@pvchMsgIN						  VARCHAR(MAX)	
--RETORNO VARIÁVEL
,@pvchMsgOUT					  VARCHAR(MAX)	= ''		OUTPUT
--RETORNO PADRÃO
,@pintNSU                         INTEGER       = 0         OUTPUT
,@ptinStatus                      TINYINT       = 0         OUTPUT    -- 0--SUCESSO 1--ERRO
,@pvchMsgErro                     VARCHAR(8000) = ''        OUTPUT
AS
SET CONCAT_NULL_YIELDS_NULL ON	    
SET NOCOUNT ON
SET LOCK_TIMEOUT 10000
  
DECLARE  @intRetCode              INTEGER      
        ,@smlAgencia1             SMALLINT      
        ,@tinPosto1               TINYINT     
        ,@intNSU1                 INTEGER      
        ,@intNSUGrupo             INTEGER      
        ,@datContabil             SMALLDATETIME      
        ,@tinPosto                TINYINT      
        ,@intTransacao            INTEGER   
        ,@intNUOperacao           INTEGER
		,@intCodigoErro           INTEGER
		,@tinTipoErro             TINYINT
        ,@tinErroNegocio          TINYINT
        ,@tinErroSistema          TINYINT
		,@smlCancelar             SMALLINT
		,@vchMsg                  VARCHAR(1000)
        ,@vchLinhaErro			  VARCHAR(1000)
        ,@tinEstadoOperacao       TINYINT
        ,@tinAcao                 TINYINT
		,@smlTratarErro           SMALLINT
        ,@vchSupervisor           VARCHAR(08)   
        ,@bitTranCount            BIT       
	    ,@vchMsgOUT               VARCHAR(MAX)
		,@smlProduto			  SMALLINT	
		,@smlContaAg			  SMALLINT	
		,@numConta                NUMERIC(12)
		,@tinTitularidade         TINYINT
		,@tinTipoLista			  TINYINT   	
        ,@vchListaCarteira		  VARCHAR(8000) 
        ,@vchDelimitador		  CHAR(01)  
        ,@vchDelimitadorFinal	  CHAR(02)  
		,@vchLista				  VARCHAR(8000) 
		,@vchXMLRet				  VARCHAR(8000) 	
		,@vchXMLEnv				  VARCHAR(8000) 
		,@intClit				  INT	
		,@datIni                  SMALLDATETIME
		,@datFim                  SMALLDATETIME

  
--Variaveis para tratamento de erro  
DECLARE @vchMsgErro VARCHAR(1000)   
       ,@intCodErro INTEGER  
 
--Valores possiveis de @tinEstado      
DECLARE @ctinIniciada        TINYINT      
       ,@ctinExecutada       TINYINT      
       ,@ctinConfirmada      TINYINT      
       ,@ctinCancelada       TINYINT      
SELECT  @ctinIniciada    = 0      
       ,@ctinExecutada   = 1      
       ,@ctinConfirmada  = 2      
       ,@ctinCancelada   = 9     
  
--Operandos para @ptinAcao                                                                                                                                                            
DECLARE @ctinValidar      TINYINT 
       ,@ctinExecutar     TINYINT 
       ,@ctinConfirmar    TINYINT
       ,@ctinCancelar     TINYINT 
       ,@ctinRegistrar    TINYINT                                                                                                                                                     
SELECT  @ctinValidar      = 1  
       ,@ctinExecutar     = 2 
       ,@ctinConfirmar    = 4 
       ,@ctinCancelar     = 8 
       ,@ctinRegistrar    = 16
 
SELECT   @ptinStatus           = 0
        ,@tinTipoErro          = 0
        ,@intCodigoErro        = 0
        ,@vchSupervisor        = ''
        ,@pvchMsgErro          = ''
        ,@smlCancelar          = 0
        ,@intNSUGrupo          = 0
        ,@vchMsg               = 'OPERAÇÃO REALIZADA COM SUCESSO'
        ,@vchLinhaErro		   = ''
        ,@smlTratarErro        = 0
        ,@tinErroNegocio       = 1
        ,@tinErroSistema       = 2
        ,@tinEstadoOperacao    = @ctinIniciada
        ,@bitTranCount         = 0

DECLARE  @cchrDELIMITADOR CHAR(02)										
		,@ctinListaPapDispAplic					TINYINT					
		,@ctinAplicNoDia						TINYINT					
		,@ctinListAplicDoCliPorTipPap			TINYINT					
		,@ctinListSalTotAplicDoCliPorTipPap     TINYINT					
		,@ctinListaCartApliCli				    TINYINT					---------- (MSG21) Solicitação de lista de carteiras nas quais o cliente tem aplicação
		,@ctinListPapApliCli					TINYINT					---------- (MSG22) Solicitação de lista de papeis nos quais o cliente tem aplicação
		,@ctinListaTipOpeCli					TINYINT
																		
SELECT   @ctinListaPapDispAplic				=	1						
		,@ctinAplicNoDia					=	2						
		,@ctinListAplicDoCliPorTipPap		=	3						
		,@ctinListSalTotAplicDoCliPorTipPap	=   4						
		,@ctinListaCartApliCli				=   5
		,@ctinListaTipOpeCli				=	6
		,@ctinListPapApliCli				=	7
													

SET @intNSU1 = 0  
SET @intNSUGrupo = 0   
SET @intTransacao = 406
 
IF @pvchEstacao = '' SELECT @pvchEstacao = HOST_NAME()

BEGIN TRY
    --Recuperando agência/posto/data contabil  
    SELECT @smlAgencia1 = hde_cod_agencia      
          ,@tinPosto1   = hde_cod_posto      
          ,@datContabil = hde_data_atual      
      FROM tbl_header_dependencia      
     WHERE hde_ind_situacao = 'A'    
 
    IF @ptinAcao NOT IN (@ctinValidar, @ctinExecutar, @ctinConfirmar, @ctinValidar | @ctinExecutar | @ctinConfirmar, @ctinExecutar | @ctinConfirmar)
    BEGIN
        RAISERROR('TIPO DE AÇÃO INVÁLIDA', 11, 1) WITH SETERROR
    END
 
	IF @ptinAcao = @ctinValidar
    BEGIN
        SELECT @tinAcao = @ctinValidar
	END
    ELSE
    BEGIN
        SELECT @tinAcao =  @ctinValidar | @ctinExecutar | @ctinConfirmar 
    END
 

	SET @smlContaAg       = JSON_VALUE(@pvchMsgIN, '$.agencia')
	SET @numConta         = JSON_VALUE(@pvchMsgIN, '$.conta')
	SET @tinTipoLista     = JSON_VALUE(@pvchMsgIN, '$.tipoLista')
	SET @vchListaCarteira = JSON_VALUE(@pvchMsgIN, '$.listaCarteira')
	SET @tinTitularidade  = JSON_VALUE(@pvchMsgIN, '$.titularidade')
  
    SET @vchDelimitador	     = '|' 
    SET @vchDelimitadorFinal = '!@'  
	SET @vchLista		     = '' 
	SET @vchXMLRet			 = ''	
	SET @vchXMLEnv			 = '' 
	SET @intClit			 = 0

    IF @@TRANCOUNT > 0
        SELECT @bitTranCount = @@TRANCOUNT
 
	------------------------------------------------------------
	-- VALIDAR/EXECUTAR TRANSACAO
	------------------------------------------------------------
     EXEC @intRetCode = dbo.SPA_CDBSolicitaLista     
          @pchrOperador                  = @pvchOperador        
         ,@pchrSupervisor                = @pvchOperador      
         ,@pchrEstacao                   = @pvchEstacao  
         ,@ptinCanal                     = @ptinCanal    
         ,@pintTransacao                 = @intTransacao  
         ,@pchrTransacaoTipo             = 'P'      
         ,@pdatContabil1                 = @datContabil  
         ,@psmlAgencia1                  = @smlAgencia1  
         ,@ptinPosto1                    = @tinPosto1  
         ,@pintNSU1                      = @intNSU1			        OUTPUT  
         ,@pintNSUGrupo1                 = @intNSUGrupo		        OUTPUT  
         ,@pdatContabil2                 = @datContabil  
         ,@psmlAgencia2                  = 0  
         ,@ptinPosto2                    = 1  
         ,@pvchLog                       = ''  
         ,@pintAutenticacao              = 0          
         ,@pbitLocal                     = 1  
         ,@ptinAcao                      = @tinAcao  
         ,@ptinEstado0                   = @ctinIniciada  
         ,@ptinEstado1                   = @ctinConfirmada 
         ,@ptinReplicacao                = 0  
         ,@pdatContabil                  = @datContabil  
         ,@pintNSUUltimo                 = 0  
         ,@pvchAreaSPA					 = ''  
         ,@pvchAreaUsuario               = ''  	 
         ,@psmlContaAg                   = @smlContaAg               
         ,@pintConta                     = @numConta  			 
		 ,@ptinTitularidade			     = @tinTitularidade
		 ,@ptinTipoLista			     = @tinTipoLista
		 ,@pvchDelimitador				 = @vchDelimitador		 
		 ,@pvchDelimitadorFinal			 = @vchDelimitadorFinal	 
		 ,@pvchListaCarteira			 = @vchListaCarteira	 
		 ,@pvchLista  				     = @vchLista			 OUTPUT
		 ,@pvchXMLRet				     = @vchXMLRet			 OUTPUT
		 ,@pvchXMLEnv				     = @vchXMLEnv			 OUTPUT
		 ,@pintClit				         = @intClit			     OUTPUT

 
        IF @@ERROR <> 0 OR @intRetCode <> 0  
        BEGIN  
	        RAISERROR('Erro na Solicitação da Lista',11,1) WITH SETERROR  
	        RETURN 1  
        END  
 

	SET @pvchMsgOUT = @vchLista

 
END TRY
BEGIN CATCH
   SELECT  @intCodigoErro  = ISNULL(ERROR_NUMBER(), 0)
  	      ,@vchMsg         = ISNULL(CAST(ERROR_MESSAGE() AS VARCHAR(1000)), '')
          ,@vchLinhaErro   = ' (' + ISNULL(CAST(ERROR_PROCEDURE() AS VARCHAR(1000)), '') + ':' + ISNULL(CAST(ERROR_LINE() AS VARCHAR(1000)), '') + ')'
 
   SELECT @smlCancelar   = 1
         ,@smlTratarErro = 1
   IF @bitTranCount = 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION
   GOTO TRATAR_ERRO
END CATCH
 
TRATAR_ERRO:
 
    SELECT @pintNSU     = @intNSUGrupo
	SET @pvchMsgErro    = (SELECT CAST(@tinTipoErro   AS VARCHAR) AS tipoErro
                                 ,CAST(@intCodigoErro AS VARCHAR) AS codErro
                                 ,@vchMsg AS msgErro
                                 ,'AUTORIZADOR' + @vchLinhaErro   AS origemErro
                             FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
 
    IF @smlTratarErro = 1
	BEGIN
        IF @bitTranCount = 0 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION
        SELECT @ptinStatus = 1
 
		IF (@intCodigoErro >= 50000 AND @intCodigoErro <= 70000)
			SELECT @tinTipoErro = @tinErroNegocio
		ELSE
			SELECT @tinTipoErro = @tinErroSistema
  
        DECLARE @vchOrigemErro		VARCHAR(200)
		       ,@vchEstacao         VARCHAR(30)
		       ,@vchLogin           VARCHAR(30)
               ,@vchLog				VARCHAR(1000)
 
        SELECT @vchEstacao		 = HOST_NAME()
	          ,@vchLogin		 = ISNULL(SUSER_NAME(), '')
		  		  
	    SELECT @vchOrigemErro = '@sps_CDBSolicitaLista   '  + @@SERVERNAME     
		      ,@vchLog	      = '@pvchOperador'             + @pvchOperador + '|'
                              + '@ptinCanal='               + CONVERT(VARCHAR, @ptinCanal) + '|'
                              + '@psmlAgencia='             + CONVERT(VARCHAR, @psmlAgencia) + '|'
                              + '@ptinPosto='               + CONVERT(VARCHAR, @ptinPosto) + '|'
                              + '@pvchEstacao='             + @pvchEstacao + '|'
                              + '@ptinAcao='                + CONVERT(VARCHAR, @ptinAcao) + '|'
							  + '@psmlProduto='             + CONVERT(VARCHAR, @smlProduto) + '|'
						      + '@pintConta='               + CONVERT(VARCHAR, @numConta) + '|'
                              + '@psmlContaAg='             + CONVERT(VARCHAR, @smlContaAg) + '|'
 
	    EXEC spx_LogSPAErro2 @psmlAgencia		   = @smlAgencia1
						    ,@ptinPosto            = @tinPosto1
						    ,@pdatContabil         = @datContabil     
						    ,@pintTransacao        = @intTransacao      
						    ,@pintNSU              = @intNSUGrupo
						    ,@ptinCanal            = @ptinCanal     
						    ,@pvchEstacao          = @vchEstacao      
						    ,@pvchOperador         = @pvchOperador     
						    ,@pvchLogin            = @vchLogin
						    ,@pvchVersao           = ''     							   
						    ,@ptinTipoErro         = 0     
						    ,@ptinTipoErroExterno  = 0    
						    ,@pintNumeroErro       = @intCodigoErro     
						    ,@pvchDescricaoErro    = @vchMsg      
						    ,@pvchOrigemErro       = @vchOrigemErro
						    ,@pvchSp1			   = @vchLog
 
		SET @pvchMsgOUT = @pvchMsgErro
	END
 
    SET @datFim = GETDATE()
 
    EXEC dbo.spx_FEPOcorrencia
                             @pdatContabil				= @datContabil
                            ,@psmlAgencia				= @psmlAgencia
                            ,@ptinPosto					= @ptinPosto
                            ,@pintNSU					= @intNSUGrupo
                            ,@ptinCanal					= @ptinCanal
                            ,@psmlProduto               = @smlProduto
                            ,@psmlContaAg				= @smlContaAg
                            ,@pnumConta					= @numConta
                            ,@pintTransacao				= @intTransacao
                            ,@pdatIni					= @datIni
                            ,@pdatFim					= @datFim
                            ,@pvchSolicitacao			= @pvchMsgIN
                            ,@pvchResposta				= @pvchMsgOUT
                            ,@pvchCodErro				= @intCodigoErro
                            ,@pvchErro					= @vchMsg
 
    --IF @@TRANCOUNT > 0 COMMIT TRANSACTION
 
    IF @@NESTLEVEL = 1
    BEGIN
        SELECT 
		 @smlProduto            AS PRODUTO
        ,@smlContaAg            AS AGENCIA        
        ,@numConta              AS CONTA     
    END  
GO


