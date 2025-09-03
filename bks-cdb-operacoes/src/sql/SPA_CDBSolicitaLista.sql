if exists (select * from sysobjects where id = object_id('SPA_CDBSolicitaLista') and sysstat & 0xf = 4 )
    drop procedure SPA_CDBSolicitaLista
GO
 
 
 
 
 
--e SPA_CDBSolicitaLista
 
--if exists (select * from sysobjects where id = object_id('SPA_CDBSolicitaLista') and sysstat & 0xf = 4 )
--    drop procedure SPA_CDBSolicitaLista
--GO
 
CREATE PROCEDURE dbo.SPA_CDBSolicitaLista
----------------------------------------------------------------------------------
--Reservado (obrigatorio em todas as transacoes) para o SPA-----------------------
--Quem (Autor)?								                            ----------
 @pchrOperador                    CHAR(08)				                ----------
,@pchrSupervisor                  CHAR(08)				                ----------
,@pchrEstacao                     VARCHAR(15)				            ----------
,@ptinCanal                       TINYINT				                ----------
--O que?								                                ----------
,@pintTransacao                   INTEGER				                ----------
,@pchrTransacaoTipo               CHAR(01) -- P-Mestre, R-Escrava       ----------
--Origem?								                                ----------
,@pdatContabil1                   SMALLDATETIME = '01/01/1900' OUTPUT	----------
,@psmlAgencia1                    SMALLINT		        	            ----------
,@ptinPosto1		  	          TINYINT			                    ----------
,@pintNSU1                        INTEGER = 0  	               OUTPUT	----------
,@pintNSUGrupo1                   INTEGER = 0 	               OUTPUT	----------
--Destino?							                                    ----------
,@pdatContabil2                   SMALLDATETIME = '01/01/1900' OUTPUT	----------
,@psmlAgencia2                    SMALLINT	                   OUTPUT   ----------
,@ptinPosto2      		          TINYINT	                   OUTPUT   ----------
--Log                   						                        ----------
,@pvchLog                         VARCHAR(255)                          ----------
,@pintAutenticacao                INTEGER = 0				   OUTPUT   ----------
--Controle         						 	                            ----------
,@pbitLocal                       BIT = 1					   OUTPUT   ----------
,@ptinAcao                        TINYINT       			            ----------
,@ptinEstado0                     TINYINT				                ----------
,@ptinEstado1                     TINYINT				                ----------
,@ptinReplicacao                  TINYINT				                ----------
,@pdatContabil                    SMALLDATETIME				            ----------
,@pintNSUUltimo                   INTEGER				                ----------
,@pvchAreaSPA                     VARCHAR(255)				            ----------
--Reservado (obrigatorio em todas as transacoes) para o Usuario  -----------------
,@pvchAreaUsuario		          VARCHAR(255) = ''		       OUTPUT 	----------
----------------------------------------------------------------------------------
--Parametros Livres (Usados pela transacao)---------------------------------------
,@psmlContaAg					  SMALLINT								----------
,@pintConta						  INTEGER								----------
,@ptinTitularidade				  TINYINT = 0							----------
,@ptinTipoLista					  TINYINT   							----------
,@pvchDelimitador				  CHAR(01) = '|'  -- Separador			----------
,@pvchDelimitadorFinal			  CHAR(02) = '!@' -- Final de registro	----------
,@pvchListaCarteira				  VARCHAR(8000) = '4'					----------
------------------- OUTPUT -------------------------------------------------------
,@pvchLista						  VARCHAR(8000) = ''		   OUTPUT   ----------
,@pvchXMLRet					  VARCHAR(8000) = ''		   OUTPUT   ----------	
,@pvchXMLEnv					  VARCHAR(8000) = ''		   OUTPUT   ----------
,@pintClit						  INT			= 0			   OUTPUT
AS																		----------
----------------------------------------------------------------------------------
--Reservado (obrigatorio em todas as transacoes)                        ----------
SET ARITHABORT ON 														----------
SET DATEFORMAT dmy														----------
SET CONCAT_NULL_YIELDS_NULL  ON 					                    ----------
SET NOCOUNT ON                                                          ----------
SET LOCK_TIMEOUT 10000                                                  ----------
SET QUOTED_IDENTIFIER ON                                                ----------
					                                                    ----------
--Valores possiveis de @ptinEstado                                      ----------
DECLARE @ctinSolicitada   TINYINT                                       ----------
       ,@ctinAutorizada   TINYINT                                       ----------
       ,@ctinConfirmada   TINYINT                                       ----------
       ,@ctinCancelada    TINYINT                                       ----------
SELECT  @ctinSolicitada   = 0                                           ----------
       ,@ctinAutorizada   = 1                                           ----------
       ,@ctinConfirmada   = 2                                           ----------
       ,@ctinCancelada    = 9                                           ----------
				                                                        ----------
--Operandos para @ptinAcao                                              ----------
DECLARE @ctinValidar      TINYINT                                       ----------
       ,@ctinExecutar     TINYINT                                       ----------
       ,@ctinConfirmar    TINYINT                                       ----------
       ,@ctinCancelar     TINYINT                                       ----------
       ,@ctinRegistrar    TINYINT                                       ----------
SELECT  @ctinValidar      = 1                                           ----------
       ,@ctinExecutar     = 2                                           ----------
       ,@ctinConfirmar    = 4                                           ----------
       ,@ctinCancelar     = 8                                           ----------
       ,@ctinRegistrar    = 16                                          ----------
																		----------	
DECLARE  @cchrDELIMITADOR CHAR(02)										----------
		,@ctinListaPapDispAplic					TINYINT					----------
		,@ctinAplicNoDia						TINYINT					----------
		,@ctinListAplicDoCliPorTipPap			TINYINT					----------
		,@ctinListSalTotAplicDoCliPorTipPap     TINYINT					----------
		,@ctinListaCartApliCli				    TINYINT					---------- (MSG21) Solicitação de lista de carteiras nas quais o cliente tem aplicação
		,@ctinListPapApliCli					TINYINT					---------- (MSG22) Solicitação de lista de papeis nos quais o cliente tem aplicação
		,@ctinListaTipOpeCli					TINYINT
																		----------
SELECT   @ctinListaPapDispAplic				=	1						----------
		,@ctinAplicNoDia					=	2						----------
		,@ctinListAplicDoCliPorTipPap		=	3						----------
		,@ctinListSalTotAplicDoCliPorTipPap	=   4						----------
		,@ctinListaCartApliCli				=   5
		,@ctinListaTipOpeCli				=	6
		,@ctinListPapApliCli				=	7
																		----------
DECLARE  @cvchAccU VARCHAR(20) -- Identifica o emissor da msg			----------
		,@cvchAccP VARCHAR(20) -- Contem a senha do emissor da msg		----------
		,@ctinhAmb TINYINT	   -- Ambiente de origem da msg (4-IBK/AUT) ----------
		
																		----------
SELECT @cvchAccU  = 'SPAUSU'											----------
	  ,@cvchAccP  = 'password'											---------- password 
  	  ,@ctinhAmb  = 4													----------
	  ,@pvchLista = ''													----------
																		----------
EXEC spx_Inicio @psmlAgencia1		                                    ----------
               ,@ptinPosto1		                                        ----------
               ,@pintTransacao		                                    ----------
               ,@ptinEstado0		                                    ----------
               ,@pdatContabil		                                    ----------
               ,@pintNSU1 	    OUTPUT                                  ----------
               ,@pintNSUGrupo1	OUTPUT                                  ----------
																		----------
IF @@error > 0 RETURN -1		                                        ----------
----------------------------------------------------------------------------------
-- PARA CONTROLE DA TRANSACAO                                           ----------
DECLARE	@bitTrancount BIT                                               ----------
SELECT  @bitTrancount = 0                                               ----------
----------------------------------------------------------------------------------
-- VARIAVEIS
DECLARE  @vchMsgXML		VARCHAR(8000)
		,@vchMsgXMLR	VARCHAR(MAX)
		,@vchRetorno	VARCHAR(1000)
		,@tinTipoErro	TINYINT 
		,@xmlTemp		XML
		,@vchMetodo		VARCHAR(100)
		,@intRetCode	INTEGER
       	,@pstrRetorno	VARCHAR(255)
		,@intClit		INT
		,@intClit1		INT
		,@intClit2		INT
		,@vchConta		VARCHAR(10)
		,@chrDV			CHAR(01)
		,@vchCodCar     VARCHAR (5)  	
		,@tinLenSeparador		TINYINT
		,@intpt					INT
		,@vchTemp				VARCHAR(1000)
 
DECLARE  @vchOcorr1                   VARCHAR(8000) 
        ,@vchOcorr2                   VARCHAR(8000) 
        ,@vchOcorr3                   VARCHAR(8000) 
        ,@vchFiller                   VARCHAR(30)   
 
----------------------------------------------------------------------------------
-- DIRECIONER PARA O HOST
----------------------------------------------------------------------------------
IF @pchrTransacaoTipo = 'P'  AND
   NOT (@psmlAgencia1 = 0 AND @ptinPosto1 = 0)
BEGIN
     SELECT @pbitLocal     = 0
       	    ,@psmlAgencia2 = 0
       	    ,@ptinPosto2   = 0
      IF @ptinAcao & 1 = 1 SELECT @ptinAcao = @ptinAcao ^ 1                 
      IF @ptinAcao & 2 = 2 SELECT @ptinAcao = @ptinAcao ^ 2                 
      IF @ptinAcao & 4 = 4 SELECT @ptinAcao = @ptinAcao ^ 4                 
      IF @ptinAcao & 8 = 8 SELECT @ptinAcao = @ptinAcao ^ 8
END
 
SELECT @ctinhAmb = CASE @ptinCanal WHEN 3 THEN 7
								   WHEN 4 THEN 7
								   WHEN 5 THEN 7								   
								   ELSE @ctinhAmb 
					END 
 
 
SELECT @cvchAccU  = vchUsuario
	  ,@cvchAccP  = vchSenha
FROM TransacaoExterna
	WHERE intTransacao = @pintTransacao  							 
 
IF @@ERROR <> 0 OR @@ROWCOUNT = 0
    BEGIN
		RAISERROR('Erro na execucao recuperção da senha -TransacaoExterna ',11,1) WITH SETERROR
		RETURN 
    END
 
----------------------------------------------------------------------------------
-- VALIDAR 
----------------------------------------------------------------------------------
IF @ptinAcao & @ctinValidar = @ctinValidar
BEGIN
 
 
	-- -- VALIDANDO CONTA DO CLIENTE NO FEP
	-- EXEC  @intRetCode = LINKED_FEP01.spabanpara.dbo.spx_ValidaContaGeral @pdatContabil		   = @pdatContabil
	--										                              ,@psmlProduto        = 1
	--																	  ,@psmlContaAg        = @psmlContaAg
	--																	  ,@pintConta          = @pintConta
	--																	  ,@psmlAgencia1       = @psmlAgencia1
	--																	  ,@ptinPosto1	       = @ptinPosto1
	--																	  ,@pintNSU1	       = @pintNSU1
	--																	  ,@psmlAgencia2	   = @psmlAgencia2
	--																	  ,@ptinPosto2	       = @ptinPosto2
	--																	  ,@ptinVia	           = 0     
	--																	  ,@ptinTitularidade   = @ptinTitularidade
	--																	  ,@pvchSenha          = ''    
	--																	  ,@pintTransacao      = 0
	--																	  ,@psmlHistorico      = 0  
	--																	  ,@ptinCanal          = @ptinCanal
	--																	  ,@ptinAcao           = @ptinAcao											  
	--																	  ,@pbitValidaSituacao = 1
            
 --   IF @@ERROR <> 0 OR @intRetCode <> 0
 --   BEGIN
	--	RAISERROR('Erro na execucao da spx_ValidaContaGeral',11,1) WITH SETERROR
	--	RETURN 1
 --   END
	
	---- RECUPERANDO CLIT CLIENTE NO FEP
	--EXEC @intRetCode = LINKED_FEP01.spabanpara.dbo.spx_Titularidade @psmlProduto  = 1
	--																,@psmlContaAg = @psmlContaAg
	--																,@pintConta   = @pintConta
	--																,@pintClit1   = @intClit1 OUTPUT
	--																,@pintClit2   = @intClit2 OUTPUT
 
	--IF @@ERROR <> 0 OR @intRetCode <> 0
 --   BEGIN
	--	RAISERROR('Erro na execucao da spx_Titularidade',11,1) WITH SETERROR
	--	RETURN 1
 --   END
	-- RECUPERANDO CLIT CLIENTE NO FEP
	EXEC @intRetCode = dbo.spx_ConsultaClit	@psmlContaAg = @psmlContaAg,
											@pintConta  = @pintConta,
											@pintClit1  = @intClit1 OUTPUT,
											@pintClit2  = @intClit2 OUTPUT
 
	IF @@ERROR <> 0 OR @intRetCode <> 0
    BEGIN
		RETURN 1
    END
 
	-- RECUPERANDO O CLIT DO CLIENTE CONFORME A TITULARIDADE
	IF @ptinTitularidade IN (0,1)
		SELECT @intClit = @intClit1
	ELSE
		SELECT @intClit = @intClit2
 
	SELECT @pintClit= @intClit	
	SELECT @vchConta= CAST(@pintConta AS VARCHAR(10))
	SELECT @chrDV = RIGHT(@vchConta,1)
	SELECT @vchConta=SUBSTRING(@vchConta,1,LEN(@vchConta)-1)
END
 
-----------------------------------------------------------------------------------
-- EXECUTAR
-----------------------------------------------------------------------------------
IF @ptinAcao & @ctinExecutar = @ctinExecutar
BEGIN	
	
	SELECT @vchConta= CAST(@pintConta AS VARCHAR(10))	
	SELECT @chrDV = RIGHT(@vchConta,1)
	SELECT @vchConta=SUBSTRING(@vchConta,1,LEN(@vchConta)-1)
	
	SELECT @tinLenSeparador=LEN(@pvchDelimitador)
    
	-- MONTA O XML CONFORME O TIPO DE LISTA SOLICITADO
	SELECT @vchMsgXML = '<CBRRFI>' 
						+ '<AccU>' + @cvchAccU + '</AccU>'
						+ '<AccP>' + @cvchAccP + '</AccP>'
						+ '<Amb>'  + CONVERT(VARCHAR(10),@ctinhAmb) + '</Amb>'
	
	IF @ptinTipoLista = @ctinListaPapDispAplic	
	BEGIN		
		
		SELECT @vchMetodo = 'ListaPapDispAplic'
		SELECT @vchMsgXML =	@vchMsgXML + '<Get>' + @vchMetodo + '</Get>'
						               + '<Params>'
									   + '<Cliente numip="'		+ CONVERT(VARCHAR(20), @pintClit )
											+ '" codagecc="'	+ CONVERT(VARCHAR(10),@psmlContaAg)
											+ '" numcc="' 		+ @vchConta 
											+ '" dvcc="'		+ @chrDV
										+ '"></Cliente>'
									   + '</Params>'
									   
		END						  
		
	IF @ptinTipoLista = @ctinAplicNoDia
	BEGIN
	
		IF CONVERT(CHAR(08), GETDATE(), 108) > (SELECT CONVERT(CHAR(08),vchString1,108) FROM parametro WHERE vchParametro='HORA_CANC_APP_DIA_CDB') 
		BEGIN
		--		IF DATEPART(HOUR,GETDATE())>=16 BEGIN		
			RAISERROR (60114, 11,  	1,  'Horario não permitido para a operação') WITH SETERROR		
			RETURN 1		
		END
 
		SELECT @vchMetodo = 'AplicNoDia' 
		SELECT @vchMsgXML =	@vchMsgXML + '<Get>' + @vchMetodo + '</Get>'
									   + '<Params>'
									   + '<Param cod="NumIP" valor="' + CONVERT(VARCHAR(20),@pintClit) + '"></Param>'
									   + '<Param cod="CodAgeCC" valor="' + CONVERT(VARCHAR(6),@psmlContaAg) + '"></Param>'
									   + '<Param cod="NumCC" valor="' + @vchConta + '"></Param>'
									   + '<Param cod="DvCC" valor="'  + @chrDV    + '"></Param>'
									   + '</Params>'
	END						  
 
	IF @ptinTipoLista =	@ctinListAplicDoCliPorTipPap
	BEGIN
		SELECT @vchMetodo='ListAplicDoCliPorTipPap'
		SELECT @vchMsgXML =	@vchMsgXML + '<Get>' + @vchMetodo + '</Get>'
										+ '<Params>'
										+ '<Param cod="NumIP" valor="' + CONVERT(VARCHAR(20),@pintClit) + '"></Param>'
										+ '<Param cod="CodAgeCC" valor="' + CONVERT(VARCHAR(6),@psmlContaAg) + '"></Param>'
										+ '<Param cod="NumCC" valor="' + @vchConta + '"></Param>'
										+ '<Param cod="DvCC" valor="'  + @chrDV    + '"></Param>'
										+ '</Params>'
 
	END
	
	IF @ptinTipoLista =	@ctinListSalTotAplicDoCliPorTipPap
	BEGIN
		
		SELECT @vchMetodo='ListSalTotAplicDoCliPorTipPap'
		SELECT @vchMsgXML =	@vchMsgXML + '<Get>' + @vchMetodo + '</Get>'
										+ '<Params>'
										+ '<Param cod="NumIP" valor="' + CONVERT(VARCHAR(20),@pintClit) + '"></Param>'
										+ '<Param cod="CodAgeCC" valor="' + CONVERT(VARCHAR(6),@psmlContaAg) + '"></Param>'
										+ '<Param cod="NumCC" valor="' + @vchConta + '"></Param>'
										+ '<Param cod="DvCC" valor="'  + @chrDV    + '"></Param>'
										+ '</Params>'
 
	END	
 
		
	
	--(MSG21) Solicitação de lista de carteiras nas quais o cliente tem aplicação
	IF @ptinTipoLista =	@ctinListaCartApliCli
	BEGIN
		
		SELECT @vchMetodo='ListaCartApliCli'
		SELECT @vchMsgXML =	@vchMsgXML + '<Get>' + @vchMetodo + '</Get>'
										+ '<Params>'
										+ '<Param cod="NumIP" valor="' + CONVERT(VARCHAR(20),@pintClit) + '"></Param>'
										+ '<Param cod="CodAgeCC" valor="' + CONVERT(VARCHAR(6),@psmlContaAg) + '"></Param>'
										+ '<Param cod="NumCC" valor="' + @vchConta + '"></Param>'
										+ '<Param cod="DvCC" valor="'  + @chrDV    + '"></Param>'
										+ '</Params>'
	END	
	
	--(MSG22) Solicitação de lista de papeis nos quais o cliente tem aplicação
	IF @ptinTipoLista =	@ctinListPapApliCli
	BEGIN
		--CodCar
        IF @pvchListaCarteira <> ''
		    SELECT @vchCodCar = @pvchListaCarteira 
        ELSE
            SELECT @vchCodCar = '-1'
		
		SELECT @vchMetodo='ListPapApliCli'
		SELECT @vchMsgXML =	@vchMsgXML + '<Get>' + @vchMetodo + '</Get>'
										+ '<Params>'
										+ '<Param cod="NumIP" valor="' + CONVERT(VARCHAR(20),@pintClit) + '"></Param>'
										+ '<Param cod="CodAgeCC" valor="' + CONVERT(VARCHAR(6),@psmlContaAg) + '"></Param>'
                                        + '<Param cod="NumCC" valor="' + @vchConta + '"></Param>'
										+ '<Param cod="CodCar" valor="'+ @vchCodCar + '"></Param>' 
										+ '</Params>'	
	END						
 
 
	--(MSG23) Solicitação de lista de tipos de operação efetuadas pelo cliente
	IF @ptinTipoLista =	@ctinListaTipOpeCli
	BEGIN
		--CodCar
        IF @pvchListaCarteira <> ''
		    SELECT @vchCodCar = @pvchListaCarteira 
        ELSE
            SELECT @vchCodCar = '-1'
		
		SELECT @vchMetodo='ListaTipOpeCli'
		SELECT @vchMsgXML =	@vchMsgXML + '<Get>' + @vchMetodo + '</Get>'
										+ '<Params>'
										+ '<Param cod="NumIP" valor="' + CONVERT(VARCHAR(20),@pintClit) + '"></Param>'
										+ '<Param cod="CodAgeCC" valor="' + CONVERT(VARCHAR(6),@psmlContaAg) + '"></Param>'
										+ '<Param cod="NumCC" valor="' + @vchConta + '"></Param>'
										+ '<Param cod="CodCar" valor="'+ @vchCodCar + '"></Param>' 
										+ '</Params>'	
	END						
 
	SELECT @vchMsgXML =	@vchMsgXML  +'</CBRRFI>'
	
	SELECT @pvchXMLEnv=@vchMsgXML
	
	-- ENVIA OS DADOS VIA ASSEMBLY PARA O CDB
	IF @vchMsgXML IS NULL
		BEGIN                                            
			RAISERROR('Erro: Enviando NULL para o WebService',11,1) WITH SETERROR
		RETURN 1
	END
 
	--GRAVA OCORRENCIA
    SELECT @vchFiller = dbo.fcx_FV('N',@psmlAgencia1,2) 				
				      + dbo.fcx_FV('N',@ptinPosto1,2) 	
				      + CONVERT(CHAR(08),@pdatContabil1,112)
				      + 'E'
         ,@vchOcorr1  = SUBSTRING(@vchMsgXML,1,8000)
 
    EXEC spx_spaocorrencia @pvchSistema = 'CDB-601'
                          ,@pnumID      = @ptinTipoLista   
                          ,@pvchFiller  = @vchFiller
                          ,@pvchOcorr1  = @vchOcorr1
 
    --DEBUG
    --IF @ptinTipoLista = @ctinListAplicDoCliPorTipPap
    --BEGIN
    --    SELECT @vchMsgXMLR = '<?xml version="1.0" encoding="ISO-8859-1" standalone="yes"?> <CBRRFI><AccU>SPAUSU</AccU><AccP>12345678</AccP><Amb>7</Amb><Get>ListAplicDoCliPorTipPap</Get><Params><Param cod="NumIP" valor="1596048"></Param><Param cod="CodAgeCC" valor="14"></Param><Param cod="NumCC" valor="215412"></Param><Param cod="DvCC" valor="9"></Param></Params><Results><Carteira codcar="4"><Papel idcompap="CDB-MAX-1080-POS-CDICE"><Oper numope="172613" numest="67977" salatu="1115,74" dtapl="05/12/2019" dtvcto="21/11/2022"/><Oper numope="172818" numest="67984" salatu="1008,56" dtapl="04/02/2020" dtvcto="19/01/2023"/><Oper numope="172888" numest="68001" salatu="1006,86" dtapl="21/02/2020" dtvcto="06/02/2023"/><Oper numope="172903" numest="68003" salatu="1308,41" dtapl="28/02/2020" dtvcto="13/02/2023"/><Oper numope="172904" numest="68004" salatu="1107,11" dtapl="28/02/2020" dtvcto="13/02/2023"/><Oper numope="172905" numest="68005" salatu="1006,47" dtapl="28/02/2020" dtvcto="13/02/2023"/></Papel><Papel idcompap="CDB-MAX-720-POS-CDICE"><Oper numope="172887" numest="68000" salatu="1006,79" dtapl="21/02/2020" dtvcto="11/02/2022"/><Oper numope="172907" numest="68007" salatu="1207,69" dtapl="28/02/2020" dtvcto="18/02/2022"/></Papel><Papel idcompap="CDB-PREM-PLUS-POS-CDICE"><Oper numope="170017" numest="67838" salatu="659,28" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170018" numest="67839" salatu="10987,87" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170019" numest="67840" salatu="12099,00" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170020" numest="67841" salatu="10987,87" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170021" numest="67842" salatu="12099,00" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170022" numest="67843" salatu="16498,63" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170023" numest="67844" salatu="16498,63" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170024" numest="67845" salatu="3293,00" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170025" numest="67846" salatu="10987,87" dtapl="05/04/2018" dtvcto="10/03/2023"/><Oper numope="170134" numest="67854" salatu="1095,53" dtapl="19/04/2018" dtvcto="24/03/2023"/><Oper numope="170195" numest="67857" salatu="10957,56" dtapl="25/04/2018" dtvcto="30/03/2023"/><Oper numope="170196" numest="67858" salatu="10957,56" dtapl="25/04/2018" dtvcto="30/03/2023"/><Oper numope="170285" numest="67862" salatu="2185,93" dtapl="08/05/2018" dtvcto="12/04/2023"/><Oper numope="170296" numest="67863" salatu="1092,76" dtapl="09/05/2018" dtvcto="13/04/2023"/><Oper numope="170309" numest="67865" salatu="1092,33" dtapl="11/05/2018" dtvcto="17/04/2023"/><Oper numope="170355" numest="67866" salatu="1091,47" dtapl="17/05/2018" dtvcto="24/04/2023"/><Oper numope="170413" numest="67871" salatu="544,78" dtapl="30/05/2018" dtvcto="04/05/2023"/><Oper numope="170450" numest="67874" salatu="10853,20" dtapl="05/06/2018" dtvcto="10/05/2023"/><Oper numope="170597" numest="67883" salatu="1079,99" dtapl="28/06/2018" dtvcto="02/06/2023"/><Oper numope="170671" numest="67887" salatu="539,30" dtapl="09/07/2018" dtvcto="13/06/2023"/><Oper numope="170677" numest="67888" salatu="1078,40" dtapl="10/07/2018" dtvcto="14/06/2023"/><Oper numope="170678" numest="67889" salatu="3235,20" dtapl="10/07/2018" dtvcto="14/06/2023"/><Oper numope="170726" numest="67894" salatu="538,61" dtapl="18/07/2018" dtvcto="22/06/2023"/><Oper numope="170867" numest="67897" salatu="2149,31" dtapl="06/08/2018" dtvcto="11/07/2023"/><Oper numope="170880" numest="67899" salatu="537,04" dtapl="09/08/2018" dtvcto="14/07/2023"/><Oper numope="170889" numest="67900" salatu="1073,87" dtapl="10/08/2018" dtvcto="17/07/2023"/><Oper numope="170890" numest="67901" salatu="3221,60" dtapl="10/08/2018" dtvcto="17/07/2023"/><Oper numope="171556" numest="67924" salatu="1595,25" dtapl="26/10/2018" dtvcto="02/10/2023"/><Oper numope="171603" numest="67931" salatu="1381,80" dtapl="31/10/2018" dtvcto="05/10/2023"/><Oper numope="171652" numest="67932" salatu="531,26" dtapl="05/11/2018" dtvcto="10/10/2023"/><Oper numope="172390" numest="67961" salatu="1533,52" dtapl="03/09/2019" dtvcto="07/08/2024"/><Oper numope="172397" numest="67964" salatu="1021,53" dtapl="10/09/2019" dtvcto="14/08/2024"/><Oper numope="172429" numest="67972" salatu="1326,10" dtapl="23/09/2019" dtvcto="27/08/2024"/><Oper numope="172435" numest="67973" salatu="509,89" dtapl="25/09/2019" dtvcto="29/08/2024"/><Oper numope="172457" numest="67975" salatu="1528,31" dtapl="03/10/2019" dtvcto="06/09/2024"/><Oper numope="172889" numest="68002" salatu="10058,49" dtapl="21/02/2020" dtvcto="27/01/2025"/><Oper numope="172909" numest="68009" salatu="10055,16" dtapl="28/02/2020" dtvcto="03/02/2025"/><Oper numope="172956" numest="68018" salatu="10041,88" dtapl="17/03/2020" dtvcto="19/02/2025"/></Papel><Papel idcompap="CDB-PREMIUM-POS-CDICE"><Oper numope="170015" numest="67836" salatu="11525,62" dtapl="05/04/2018" dtvcto="22/03/2021"/><Oper numope="170136" numest="67856" salatu="2180,41" dtapl="20/04/2018" dtvcto="05/04/2021"/><Oper numope="170251" numest="67860" salatu="1088,74" dtapl="02/05/2018" dtvcto="16/04/2021"/><Oper numope="170252" numest="67861" salatu="2177,49" dtapl="02/05/2018" dtvcto="16/04/2021"/><Oper numope="170297" numest="67864" salatu="1087,72" dtapl="09/05/2018" dtvcto="23/04/2021"/><Oper numope="170412" numest="67870" salatu="542,32" dtapl="30/05/2018" dtvcto="14/05/2021"/><Oper numope="170525" numest="67881" salatu="647,76" dtapl="19/06/2018" dtvcto="04/06/2021"/><Oper numope="170526" numest="67882" salatu="323,88" dtapl="19/06/2018" dtvcto="04/06/2021"/><Oper numope="170623" numest="67884" salatu="1185,61" dtapl="02/07/2018" dtvcto="16/06/2021"/><Oper numope="170624" numest="67885" salatu="323,35" dtapl="02/07/2018" dtvcto="16/06/2021"/><Oper numope="170760" numest="67895" salatu="644,71" dtapl="25/07/2018" dtvcto="09/07/2021"/><Oper numope="170866" numest="67896" salatu="536,49" dtapl="06/08/2018" dtvcto="21/07/2021"/><Oper numope="171285" numest="67919" salatu="1064,66" dtapl="05/10/2018" dtvcto="20/09/2021"/><Oper numope="171555" numest="67923" salatu="849,58" dtapl="26/10/2018" dtvcto="11/10/2021"/><Oper numope="171557" numest="67925" salatu="1274,36" dtapl="26/10/2018" dtvcto="11/10/2021"/><Oper numope="171558" numest="67926" salatu="1486,76" dtapl="26/10/2018" dtvcto="11/10/2021"/><Oper numope="171559" numest="67927" salatu="2867,31" dtapl="26/10/2018" dtvcto="11/10/2021"/><Oper numope="171581" numest="67928" salatu="4777,96" dtapl="29/10/2018" dtvcto="13/10/2021"/><Oper numope="171582" numest="67929" salatu="5096,49" dtapl="29/10/2018" dtvcto="13/10/2021"/><Oper numope="171589" numest="67930" salatu="4883,26" dtapl="30/10/2018" dtvcto="14/10/2021"/><Oper numope="171774" numest="67936" salatu="529,64" dtapl="19/11/2018" dtvcto="03/11/2021"/><Oper numope="171775" numest="67937" salatu="635,57" dtapl="19/11/2018" dtvcto="03/11/2021"/><Oper numope="171776" numest="67938" salatu="741,49" dtapl="19/11/2018" dtvcto="03/11/2021"/><Oper numope="171777" numest="67939" salatu="847,43" dtapl="19/11/2018" dtvcto="03/11/2021"/><Oper numope="172388" numest="67960" salatu="1737,50" dtapl="02/09/2019" dtvcto="17/08/2022"/><Oper numope="172447" numest="67974" salatu="1731,91" dtapl="01/10/2019" dtvcto="15/09/2022"/><Oper numope="172805" numest="67981" salatu="5036,84" dtapl="31/01/2020" dtvcto="16/01/2023"/><Oper numope="172815" numest="67982" salatu="2215,96" dtapl="03/02/2020" dtvcto="18/01/2023"/><Oper numope="172817" numest="67983" salatu="2014,28" dtapl="04/02/2020" dtvcto="19/01/2023"/><Oper numope="172820" numest="67985" salatu="2014,28" dtapl="04/02/2020" dtvcto="19/01/2023"/><Oper numope="172826" numest="67986" salatu="1007,03" dtapl="05/02/2020" dtvcto="20/01/2023"/><Oper numope="172828" numest="67987" salatu="503,52" dtapl="05/02/2020" dtvcto="20/01/2023"/><Oper numope="172835" numest="67988" salatu="1006,91" dtapl="06/02/2020" dtvcto="23/01/202
    --END
    --ELSE
    --BEGIN
	    EXEC SPX_CDBIntegracaoDLL -- @pvchGet		 = @vchMetodo	
						        @pvchXML	 = @vchMsgXML	
						       ,@pvchXMLR	 = @vchMsgXMLR OUTPUT
						       ,@pstrRetorno = @vchRetorno OUTPUT
    --END                            
    --FIM DEBUG
 
		
	IF @vchRetorno <> ''
	BEGIN                                          
		INSERT INTO spaocorrencia
			SELECT	 GETDATE()
					,CONVERT(CHAR(08),GETDATE(),112)
					,'CDB-601' 
					,@ctinExecutar
					,dbo.fcx_FV('N',@psmlAgencia1,2) 				
					+ dbo.fcx_FV('N',@ptinPosto1,2) 	
					+ CONVERT(CHAR(08),@pdatContabil1,112)
					+ 'R'
					,SUBSTRING(@vchRetorno,1,8000)
		RAISERROR('SISTEMA CDB/RDB INDISPONÍVEL',11,1) WITH SETERROR  
		--RAISERROR(@vchRetorno,11,1) WITH SETERROR
		RETURN 1
	END
 
 
    SELECT @vchFiller = dbo.fcx_FV('N',@psmlAgencia1,2) 				
				      + dbo.fcx_FV('N',@ptinPosto1,2) 	
				      + CONVERT(CHAR(08),@pdatContabil1,112)
				      + 'R'
         ,@vchOcorr1  = SUBSTRING(@vchMsgXMLR,1,8000)
         ,@vchOcorr2  = SUBSTRING(@vchMsgXMLR,8001,8000)
         ,@vchOcorr3  = SUBSTRING(@vchMsgXMLR,16001,8000)
 
    EXEC spx_spaocorrencia @pvchSistema = 'CDB-601'
                          ,@pnumID      = @ptinTipoLista   
                          ,@pvchFiller  = @vchFiller
                          ,@pvchOcorr1  = @vchOcorr1
                          ,@pvchOcorr2  = @vchOcorr2
                          ,@pvchOcorr3  = @vchOcorr3
 
	IF CHARINDEX('?>',@vchMsgXMLR ) > 0
		SELECT @vchMsgXMLR=STUFF(@vchMsgXMLR,1,CHARINDEX('?>',@vchMsgXMLR )+1 ,'')
 
	-- TRATA O XML CASO TENHA ERRO
	EXEC SPX_CDBConsultaErro @pvchMsgXMLR	= @vchMsgXMLR
							 ,@ptinTipoErro	= @tinTipoErro OUTPUT
							 ,@pvchDescErro	= @vchRetorno  OUTPUT
	
	 
	 --select @vchRetorno=left(@vchRetorno,4)
	IF @vchRetorno <> '' BEGIN
		IF @tinTipoErro=1 --erro de neg                             			
			RAISERROR (60114, 11,  	1,  @vchRetorno) WITH SETERROR
		ELSE 
			RAISERROR(@vchRetorno,11,1) WITH SETERROR
		RETURN 1
	END
	
	SELECT @pvchXMLRet = @vchMsgXMLR
	SELECT @xmlTemp= CONVERT(XML,@vchMsgXMLR)
 
	-- MONTA A LISTA DE RETORNO COMFORME O SOLICITADO	
	IF @ptinTipoLista = @ctinListaPapDispAplic
		SELECT @pvchLista = @pvchLista + Tab.Col.value('@cod_car','VARCHAR(100)')    + @pvchDelimitador
									   + Tab.Col.value('@id_com_pap','VARCHAR(100)') + @pvchDelimitador
									   + Tab.Col.value('@nom_car','VARCHAR(100)')    + @pvchDelimitador 
									   + Tab.Col.value('@vr_min','VARCHAR(100)')     + @pvchDelimitador -- Valor minimo 1ra app
									   + Tab.Col.value('@vr_min_adi','VARCHAR(100)') + @pvchDelimitador -- Valor minimo /p nova app
									   + Tab.Col.value('@vr_rsg_min','VARCHAR(100)') + @pvchDelimitador -- Valor minimo /p nova app
									   + Tab.Col.value('@vr_min_pmc','VARCHAR(100)') + @pvchDelimitador -- Valor minimo /p nova app									 
									   + Tab.Col.value('@sldcli','VARCHAR(100)') + @pvchDelimitador -- Valor minimo /p nova app										   
									   + Tab.Col.value('@pubasemi','VARCHAR(100)') + @pvchDelimitador --preço unitário básico de emissão do papel.
 
									   + @pvchDelimitadorFinal
		FROM @xmlTemp.nodes('/CBRRFI/Results/Result') Tab(Col)
		
	
	--4|3981624|CDB-PREM-PLUS-POS-CDICE|1|20.000,00|14/02/2013|14/02/2015|123456|!@	
	--Lista-2 
	IF @ptinTipoLista = @ctinAplicNoDia
		SELECT @pvchLista = @pvchLista +
									    CASE WHEN Tab.Col.value('@cod','VARCHAR(100)') = 'NSU_AUT' THEN
											  Tab.Col.value('@valor','VARCHAR(100)')  + @pvchDelimitador + @pvchDelimitadorFinal																						 
										      WHEN Tab.Col.value('@cod','VARCHAR(100)') = 'DatVcto' THEN
											  CONVERT(VARCHAR, GETDATE(), 103)		  + @pvchDelimitador
											+ Tab.Col.value('@valor','VARCHAR(100)')  + @pvchDelimitador										 
										 ELSE 
											Tab.Col.value('@valor','VARCHAR(100)')    + @pvchDelimitador
										 END  															 
		FROM @xmlTemp.nodes('/CBRRFI/Results/Result/Oper') Tab(Col)
		WHERE Tab.Col.value('@cod','VARCHAR(100)') <> 'HORA'
 
	--4|CDB-PREM-PLUS-POS-CDICE|3981502|102536|12.480,00|11/02/2013|12/03/2025|!@
	--Lista-3
	IF @ptinTipoLista =	@ctinListAplicDoCliPorTipPap
		SELECT	@pvchLista = @pvchLista+ Tab.Col.value('../../@codcar','VARCHAR(100)')	+ @pvchDelimitador				               
									   + Tab.Col.value('../@idcompap','VARCHAR(100)')	+ @pvchDelimitador	
						               + Tab.Col.value('@numope','VARCHAR(100)')		+ @pvchDelimitador
									   + Tab.Col.value('@numest','VARCHAR(100)')		+ @pvchDelimitador
						               + Tab.Col.value('@salatu','VARCHAR(100)')		+ @pvchDelimitador
									   + Tab.Col.value('@dtapl','VARCHAR(100)')			+ @pvchDelimitador
						               + Tab.Col.value('@dtvcto','VARCHAR(100)')		+ @pvchDelimitador + @pvchDelimitadorFinal
		FROM @xmlTemp.nodes('/CBRRFI/Results/Carteira/Papel/Oper') Tab(Col)
 
	--4|CDB-PREM-PLUS-POS-CDICE|39.550,26|100,00|100,00|!@
	--Lista-4(msg15)
	--**aguardando 2 campos novos
	IF @ptinTipoLista = @ctinListSalTotAplicDoCliPorTipPap
		SELECT @pvchLista = @pvchLista + Tab.Col.value('../@codcar','VARCHAR(100)')		+ @pvchDelimitador
									   --+ Tab.Col.value('../@valor','VARCHAR(100)') + @pvchDelimitador
									   + Tab.Col.value('@idcompap','VARCHAR(100)')		+ @pvchDelimitador
	    							   + Tab.Col.value('@salatu','VARCHAR(100)')		+ @pvchDelimitador 
									   + Tab.Col.value('@salbr','VARCHAR(100)')			+ @pvchDelimitador 		
									   + Tab.Col.value('@vr_rsg_min','VARCHAR(100)')	+ @pvchDelimitador 
									   + Tab.Col.value('@vr_min_pmc','VARCHAR(100)')	+ @pvchDelimitador 										   								   								   
									   + @pvchDelimitadorFinal									   
		FROM @xmlTemp.nodes('/CBRRFI/Results/Carteira/Papel') Tab(Col)
 
 
	IF @ptinTipoLista =	@ctinListaCartApliCli
		SELECT @pvchLista=@pvchLista  +  Tab.Col.value('@cod_car', 'VARCHAR(100)')  + @pvchDelimitador
									  +	 Tab.Col.value('@nom_car','VARCHAR(100)')	+ @pvchDelimitador + @pvchDelimitadorFinal		    							   
		FROM @xmlTemp.nodes('/CBRRFI/Results/Result') Tab(Col)
 
	IF @ptinTipoLista =	@ctinListaTipOpeCli
		SELECT @pvchLista=@pvchLista  +  Tab.Col.value('@id_ope', 'VARCHAR(100)')  + @pvchDelimitador
									  +	 Tab.Col.value('@nom_ope','VARCHAR(100)')	+ @pvchDelimitador + @pvchDelimitadorFinal		    							   
		FROM @xmlTemp.nodes('/CBRRFI/Results/Result') Tab(Col)
 
	IF @ptinTipoLista =	@ctinListPapApliCli
		SELECT @pvchLista = @pvchLista + @vchCodCar + @pvchDelimitador
                                       + Tab.Col.value('@idcompap', 'VARCHAR(100)')  + @pvchDelimitador + @pvchDelimitadorFinal		    							   
		FROM @xmlTemp.nodes('/CBRRFI/Results/Result') Tab(Col)
 
	IF @pvchLista = ''
	BEGIN
		RAISERROR('NÃO EXISTEM DADOS PARA CONSULTA',11,1) WITH SETERROR
		RETURN 1
	END
END
----------------------------------------------------------------------------------
-- REGISTRAR 
----------------------------------------------------------------------------------
IF @ptinAcao & @ctinRegistrar = @ctinRegistrar
BEGIN
 
	SELECT @pvchLog = CONVERT(VARCHAR(20), @psmlContaAg)
		              + CONVERT(VARCHAR(20), @pintConta)
					  + CONVERT(VARCHAR(20), @ptinTitularidade)
					  + CONVERT(VARCHAR(20), @ptinTipoLista)
					  + @pvchLista
 
    EXEC @intRetCode = spx_Log @pchrOperador       = @pchrOperador
                               ,@pchrSupervisor    = @pchrSupervisor
							   ,@pchrEstacao       = @pchrEstacao
							   ,@ptinCanal		   = @ptinCanal
							   ,@pintTransacao     = @pintTransacao
							   ,@pchrTransacaoTipo = @pchrTransacaoTipo
							   ,@pdatContabil1     = @pdatContabil1		OUTPUT
							   ,@psmlAgencia1      = @psmlAgencia1
							   ,@ptinPosto1	       = @ptinPosto1
							   ,@pintNSU1          = @pintNSU1			OUTPUT
							   ,@pintNSUGrupo1     = @pintNSUGrupo1		OUTPUT
							   ,@pdatContabil2     = @pdatContabil2		OUTPUT
							   ,@psmlAgencia2      = @psmlAgencia2
							   ,@ptinPosto2        = @ptinPosto2
							   ,@pvchLog           = @pvchLog   
							   ,@pbitLocal         = @pbitLocal
							   ,@ptinAcao          = @ptinAcao
							   ,@ptinEstado0       = @ptinEstado0
							   ,@ptinEstado1       = @ptinEstado1
							   ,@ptinReplicacao    = @ptinReplicacao
							   ,@pintNSUUltimo     = @pintNSUUltimo
							   ,@pvchAreaSPA       = @pvchAreaSPA
							   ,@pintAutenticacao  = @pintAutenticacao	 OUTPUT
							   ,@psmlHistorico     = 0
							   ,@pintDocumento     = 0
							   ,@pnumDinheiro      = 0
							   ,@pnumCheque        = 0
							   ,@pnumCPMF          = 0
							   ,@pintConta         = @pintConta
                               ,@pbitFEP           = 0 
 
	IF @@ERROR <> 0 OR @intRetCode <> 0
	BEGIN
		IF @bitTrancount = 1 ROLLBACK TRANSACTION
			RETURN 1
	END
END
GO

