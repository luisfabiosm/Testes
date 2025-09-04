# PIX Pagador - Microserviço de Pagamentos PIX

## 📋 Sobre o Projeto

O **PIX Pagador** é um microserviço .NET 8 desenvolvido seguindo os princípios de **Clean Architecture** (Arquitetura Limpa) e **Arquitetura Hexagonal**. O projeto implementa funcionalidades completas para processamento de pagamentos e devoluções PIX, oferecendo uma solução robusta, escalável e de alta performance para instituições financeiras.

## 🚀 Principais Funcionalidades

### **Operações de Pagamento PIX**
- **Registrar Ordem de Pagamento**: Inicia uma nova transação PIX
- **Efetivar Ordem de Pagamento**: Confirma e processa a transação
- **Cancelar Ordem de Pagamento**: Cancela uma transação em andamento

### **Operações de Devolução PIX**
- **Requisitar Ordem de Devolução**: Inicia processo de devolução
- **Efetivar Ordem de Devolução**: Confirma e processa a devolução
- **Cancelar Ordem de Devolução**: Cancela uma devolução em andamento

## 🏗️ Arquitetura

O projeto segue uma arquitetura modular e bem definida:

```
src/pix-pagador/
├── Domain/                          # Lógica de domínio (regras de negócio)
│   ├── Core/                        # Núcleo do domínio
│   ├── UseCases/                    # Casos de uso específicos
│   └── Services/                    # Serviços de domínio
├── Adapters/                        # Adaptadores de infraestrutura
│   ├── Inbound/                     # Adaptadores de entrada
│   │   └── WebApi/                  # Controllers e endpoints REST
│   └── Outbound/                    # Adaptadores de saída
│       ├── Database/SQL/            # Repositórios SQL (SQL Server/PostgreSQL)
│       ├── Database/NoSQL/          # Repositórios NoSQL (MongoDB)
│       ├── Messaging/               # Adaptadores de mensageria
│       └── Metrics/                 # Adaptadores de métricas
└── Configuration/                   # Configurações e DI
```

## 🛠️ Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **Dapper** - Micro ORM para acesso a dados
- **SQL Server** - Banco de dados relacional
- **Prometheus** - Métricas (opcional)
- **Docker** - Containerização
- **Mediator Pattern** - Mediação de requests
- **Result Pattern** - Tratamento de resultados sem exceptions

## 📡 Endpoints da API

### **Pagamentos PIX**
```http
POST /soa/pix/api/v1/debito/registrar
POST /soa/pix/api/v1/debito/efetivar
POST /soa/pix/api/v1/debito/cancelar
```

### **Devoluções PIX**
```http
POST /soa/pix/api/v1/devolucao/requisitar
POST /soa/pix/api/v1/devolucao/efetivar
POST /soa/pix/api/v1/devolucao/cancelar
```

## 🔧 Configuração e Instalação

### **Pré-requisitos**
- .NET 8 SDK
- SQL Server
- Docker (opcional)


## ⚙️ Configuração

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PixPagador;Trusted_Connection=true;"
  },
  "DatabaseSettings": {
    "CommandTimeout": 30,
    "RetryCount": 3
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## 🎯 Padrões Implementados

### **Clean Architecture**
- Separação clara entre domínio, aplicação e infraestrutura
- Independência de frameworks externos
- Testabilidade maximizada

### **Mediator**
- Separação de comandos e consultas
- Mediação centralizada de requests
- Handler pattern para casos de uso

### **Result Pattern**
- Tratamento de erros sem exceptions para controle de fluxo
- Performance otimizada (40-60% de melhoria)
- Código mais legível e previsível

### **Repository Pattern**
- Abstração de acesso a dados
- Suporte a múltiplos tipos de banco
- Implementação com retry automático


## 📈 Performance e Otimizações

- **Source Generators** para serialização JSON otimizada
- **ValueTask** para operações assíncronas
- **Result Pattern** eliminando exceptions de controle de fluxo
- **Dapper** para acesso otimizado a dados
- **Connection pooling** configurado

## 🔐 Segurança

- Autenticação e autorização configuradas
- Validation pipeline integrado
- Correlation ID para rastreabilidade
- Logs estruturados para auditoria

## 📋 Exemplos de Uso

### **Registrar Pagamento PIX**
```json
POST /soa/pix/api/v1/debito/registrar
{
  "idReqSistemaCliente": "12345",
  "valor": 100.00,
  "chave": "user@example.com",
  "dadosPagador": {
    "cpfCnpj": "12345678901",
    "nome": "João Silva"
  },
  "dadosRecebedor": {
    "cpfCnpj": "98765432100",
    "nome": "Maria Santos"
  }
}
```

### **Resposta de Sucesso**
```json
{
  "success": true,
  "data": {
    "endToEndId": "E12345678202412041200202412040001",
    "status": "REGISTRADO"
  },
  "message": "Ordem de pagamento registrada com sucesso",
  "correlationId": "REG-550e8400-e29b-41d4-a716-446655440000"
}
```

## 👨‍💻 Autor

**Fabio Magalhaes** - Arquiteto de Software
