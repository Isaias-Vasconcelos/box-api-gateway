
# 📦 BOX - API Gateway Dinâmico com .NET

**BOX** é um **API Gateway** construído do zero em **ASP.NET Core**, com foco em **orquestração**, **segurança**, **resiliência**, e **configuração dinâmica** via YAML. Ele centraliza o acesso a múltiplas APIs internas e se comunica com elas via rede Docker interna. Todo o comportamento do gateway é controlado pelo `service.yaml`.

---

## 🚀 Funcionalidades

- ✅ Leitura dinâmica de configurações via YAML
- 🔐 Autenticação JWT
- 🔁 Retry automático
- 🛡️ Rate Limiting
- 🚀 Cache
- ⚠️ Tratamento centralizado de erros
- 📂 Comunicação segura via rede Docker interna
- 🧠 Arquitetura extensível

---

## 🧩 Exemplo Completo de `service.yaml`

```yaml
version: 1.0                       # Versão do arquivo
appName: SERVICE_MASTER            # Nome do gateway

services:
  servicePost:                     # Nome do serviço (usado internamente)
    endpoints:
      getAllPosts:
        path: /posts               # Caminho exposto pelo gateway
        method: GET
      getPostsById:
        path: /posts/[id]          # Suporte a parâmetros no path
        method: GET
      createPost:
        path: /posts
        method: POST
    config:
      origin: http://service-post:5001  # URL interna (nome do serviço no docker)

  serviceUser:
    endpoints:
      getAllUsers:
        path: /users
        method: GET
      getUserById:
        path: /users/[id]
        method: GET
      postUser:
        path: /users
        method: POST
    config:
      origin: http://service-user:5002

  serviceCep:
    endpoints:
      getAddressByCep:
        path: /ws/[cep]/[type]
        method: GET
    config:
      origin: http://service-cep:5004

config:
  accessToken: 123456                    # Token de acesso básico (Gerado no site).

  auth:                                  # Autenticação JWT
    origin: http://service-auth:5003     # Serviço que valida o token JWT
    path: /verify-user

  useCache: false                        # (Reservado para cache futuro)
  cache:
    periodic: 1

  useRateLimit: false                    # Habilita rate limiting
  rateLimit:
    permitLimit: 10
    windowTypeTime: HOURS
    windowTime: 2
    queueProcessingOrder: OLDEST_FIRST
    queueLimit: 0

  httpRetry:                             # Configurações de retry
    delayTime: 5
    delayTypeTime: SECONDS
    maxRetryAttempts: 6
```

---

## 🐳 Exemplo Completo de `docker-compose.yml`

```yaml
version: '3.8'
services:

  gateway:
    image: isaiasdevback/box:latest           # Imagem do Gateway publicada no Docker Hub
    ports:
      - "8080:8080"
    volumes:
      - ./service.yaml:/app/service.yaml  # Monta o arquivo de config
    depends_on:
      - service-post
      - service-user
      - service-cep
      - service-auth
      - service-stats
    networks:
      - internal

  service-post:
    image: seuusuario/service-post:latest
    networks:
      - internal

  service-user:
    image: seuusuario/service-user:latest
    networks:
      - internal

  service-cep:
    image: seuusuario/service-cep:latest
    networks:
      - internal

  service-auth:
    image: seuusuario/service-auth:latest
    networks:
      - internal

  service-stats:
    image: seuusuario/service-stats:latest
    networks:
      - internal

networks:
  internal:
    driver: bridge
```

---

## ▶️ Como executar

1. Tenha o Docker instalado.
2. Crie um arquivo `service.yaml` com base no exemplo acima.
3. Crie um `docker-compose.yml` com os serviços internos.
4. Rode:

```bash
docker-compose up -d
```

O gateway ficará disponível em:

```
http://localhost:8080
```

---

## 🔐 Autenticação JWT

- Cada requisição com JWT precisa de um header:

```http
Authorization: Bearer <seu_token_aqui>
```

- O gateway valida o usuário chamando o endpoint que foi definido no serviço `auth`.
- O serviço definido em `auth`, deve retornar um json que contenha a propriedade `{ "isAuthenticated": true or false }`, caso seja `true` ele gera um Bearer Token e adiciona o seguinte header na requisição:
  - `X-User-Id`
    
---

## 🔁 Retry com Polly

- Configurado via `httpRetry` no `service.yaml`
- Em caso de erro temporário (timeout, 5xx), o gateway tentará novamente
- Respeita o número máximo de tentativas e tempo entre elas

---

## 🛡️ Rate Limiting

- Quando ativado (`useRateLimit: true`), limita o número de requisições por janela de tempo.
- Requisições acima do limite recebem código HTTP 429.

---

## ⚠️ Tratamento de Erros

- Todos os erros passam por middlewares centralizados
- Erros da API retornam mensagens padronizadas (ex: `{"origin":"http://service.m1:5000", "enpoint": "/users" , "statusCode": 500 , "data": { resposta de erro do seu serviço }}`)

---

## 🧪 Exemplo de Requisição

```bash
curl http://localhost:8080/users   -H "Authorization: Bearer SEU_TOKEN_JWT"
```

---

# 🔑 Token de Acesso Necessário para o API Gateway

Para utilizar o **API Gateway**, é necessário configurar o **AccessToken** no arquivo `service.yaml`. Este token é gerado diretamente no **site de autenticação** e é essencial para garantir que somente usuários cadastrados em nossa base possam acessar.

---

## 📬 Contato

Desenvolvido por Isaías Vasconcelos.  
Sinta-se livre para abrir issues ou sugerir melhorias.

---

## 🧠 Por que BOX?

> Porque tudo cabe aqui dentro.  
> Centralize, controle e orquestre suas APIs com segurança e inteligência.
