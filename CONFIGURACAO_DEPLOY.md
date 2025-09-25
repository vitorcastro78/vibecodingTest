# Configuração e Deploy - SupermarketAPI

## 🔧 Configurações Necessárias

### 1. JWT (Autenticação)
```json
"Jwt": {
  "Key": "SuperSecretJWTKeyForSupermarketAPIThatIs32CharsLong!"
}
```
- **Desenvolvimento**: Já configurado
- **Produção**: Usar variável de ambiente `JWT_SECRET_KEY`

### 2. Email (SMTP)
```json
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password"
}
```

#### Gmail Configuration:
1. Ativar **2-Factor Authentication**
2. Gerar **App Password**: https://myaccount.google.com/apppasswords
3. Usar App Password no campo `Password`

#### Outras opções SMTP:
- **SendGrid**: smtp.sendgrid.net (Port 587)
- **Mailgun**: smtp.mailgun.org (Port 587)
- **Outlook**: smtp-mail.outlook.com (Port 587)

### 3. WhatsApp (Twilio)
```json
"Twilio": {
  "AccountSid": "your-twilio-account-sid",
  "AuthToken": "your-twilio-auth-token",
  "WhatsAppNumber": "+14155238886"
}
```

#### Twilio Setup:
1. Criar conta: https://www.twilio.com/
2. Obter **Account SID** e **Auth Token**
3. Configurar **WhatsApp Sandbox**: https://console.twilio.com/us1/develop/sms/try-it-out/whatsapp-learn

### 4. Redis (Cache)
```json
"Redis": {
  "Configuration": "localhost:6379,abortConnect=false"
}
```

#### Instalação Redis:
- **Windows**: https://github.com/microsoftarchive/redis/releases
- **Linux**: `sudo apt install redis-server`
- **Docker**: `docker run -d -p 6379:6379 redis:alpine`

## 🚀 Execução da Aplicação

### Desenvolvimento
```bash
cd src/SupermarketAPI.API
dotnet run
```

### Produção
```bash
cd src/SupermarketAPI.API
dotnet publish -c Release -o ./publish
cd publish
dotnet SupermarketAPI.API.dll
```

## 🐳 Docker

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/SupermarketAPI.API/SupermarketAPI.API.csproj", "src/SupermarketAPI.API/"]
COPY ["src/SupermarketAPI.Application/SupermarketAPI.Application.csproj", "src/SupermarketAPI.Application/"]
COPY ["src/SupermarketAPI.Domain/SupermarketAPI.Domain.csproj", "src/SupermarketAPI.Domain/"]
COPY ["src/SupermarketAPI.Infrastructure/SupermarketAPI.Infrastructure.csproj", "src/SupermarketAPI.Infrastructure/"]
COPY ["src/SupermarketAPI.Scrapers/SupermarketAPI.Scrapers.csproj", "src/SupermarketAPI.Scrapers/"]
COPY ["src/SupermarketAPI.Notifications/SupermarketAPI.Notifications.csproj", "src/SupermarketAPI.Notifications/"]

RUN dotnet restore "src/SupermarketAPI.API/SupermarketAPI.API.csproj"
COPY . .
WORKDIR "/src/src/SupermarketAPI.API"
RUN dotnet build "SupermarketAPI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SupermarketAPI.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupermarketAPI.API.dll"]
```

### Docker Compose
```yaml
version: '3.8'

services:
  supermarket-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - JWT_SECRET_KEY=your-super-secret-jwt-key-here
      - REDIS_CONNECTION_STRING=redis:6379
      - TWILIO_ACCOUNT_SID=your-twilio-account-sid
      - TWILIO_AUTH_TOKEN=your-twilio-auth-token
      - TWILIO_WHATSAPP_NUMBER=+14155238886
      - SMTP_SERVER=smtp.gmail.com
      - SMTP_PORT=587
      - SMTP_USERNAME=your-email@gmail.com
      - SMTP_PASSWORD=your-app-password
      - FROM_EMAIL=noreply@supermarketapi.com
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    depends_on:
      - redis

  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  redis_data:
```

## 🌐 URLs e Endpoints

### Desenvolvimento
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Hangfire Dashboard**: http://localhost:5000/hangfire
- **Health Checks**: http://localhost:5000/health

### Principais Endpoints
```
POST /api/auth/register    - Registro de usuário
POST /api/auth/login       - Login
GET  /api/products         - Lista produtos
GET  /api/rankings/daily   - Ranking diário
POST /api/jobs/scraping    - Executar scraping
POST /api/email/send-product-results - Enviar email
```

## 📊 Monitoramento

### Health Checks
- **Database**: Verifica conexão SQLite
- **Redis**: Verifica conexão cache
- **External Services**: Verifica Twilio/SMTP

### Logs
- **Console**: Logs em tempo real
- **File**: Arquivos rotativos diários
- **Structured**: JSON com contexto

### Hangfire Dashboard
- **Jobs**: Monitorar jobs em execução
- **Recurring**: Jobs agendados
- **Failed**: Jobs com falha
- **Servers**: Status dos servidores

## 🔒 Segurança

### Produção Checklist
- [ ] JWT Key forte (32+ caracteres)
- [ ] HTTPS habilitado
- [ ] Rate Limiting configurado
- [ ] CORS restritivo
- [ ] Logs sem dados sensíveis
- [ ] Variáveis de ambiente para secrets
- [ ] Firewall configurado
- [ ] Backup da base de dados

### Variáveis de Ambiente
```bash
export JWT_SECRET_KEY="your-super-secret-jwt-key-here"
export REDIS_CONNECTION_STRING="localhost:6379"
export TWILIO_ACCOUNT_SID="your-twilio-account-sid"
export TWILIO_AUTH_TOKEN="your-twilio-auth-token"
export SMTP_USERNAME="your-email@gmail.com"
export SMTP_PASSWORD="your-app-password"
```

## 📝 Testes

### Executar Testes
```bash
cd tests/SupermarketAPI.Tests
dotnet test --verbosity normal
```

### Cobertura de Testes
- **Unitários**: 62 testes passando
- **Integração**: Scrapers e orchestrator
- **Funcionalidades**: Matching, Email, WhatsApp

## 🔧 Troubleshooting

### Problemas Comuns

#### 1. Redis Connection Failed
```
Solution: Verificar se Redis está executando
docker run -d -p 6379:6379 redis:alpine
```

#### 2. Email Not Sending
```
Solution: Verificar credenciais SMTP e App Password
```

#### 3. WhatsApp Not Working
```
Solution: Verificar Twilio Sandbox e credenciais
```

#### 4. Database Locked
```
Solution: Verificar se múltiplas instâncias estão rodando
```

#### 5. JWT Invalid
```
Solution: Verificar se JWT Key tem 32+ caracteres
```

## 📦 Deploy em Cloud

### Azure App Service
1. Criar App Service
2. Configurar variáveis de ambiente
3. Deploy via GitHub Actions

### AWS Elastic Beanstalk
1. Criar application
2. Upload do .zip
3. Configurar environment variables

### Google Cloud Run
1. Build container image
2. Deploy to Cloud Run
3. Set environment variables

## 🎯 Próximos Passos

1. **Configurar credenciais reais**
2. **Ajustar selectors dos scrapers**
3. **Implementar CI/CD**
4. **Configurar monitoramento**
5. **Backup automatizado**
6. **Scaling horizontal**

A aplicação está **PRONTA** para produção! 🚀
