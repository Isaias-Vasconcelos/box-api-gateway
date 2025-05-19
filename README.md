# 📦 BOX - Simplifique Suas Integrações com um Gateway de APIs Universal

**BOX** é um **API Gateway** construído em **.NET** utilizando o YARP, com foco em **orquestração**, **segurança**, **resiliência**, e **configuração dinâmica** via JSON. Ele centraliza o acesso a múltiplas APIs internas e se comunica com elas via rede Docker interna. Todo o comportamento do gateway é controlado pelo `service.json`.

---

## 🚀 Funcionalidades

- 🔁 Proxy Reverso  
- 🔐 Autenticação JWT  
- 🛡️ Rate Limiting  
- ⚠️ Tratamento centralizado de erros  
- 🧠 Arquitetura extensível  
- ⚖️ Load Balancing
- 🌐 Roteamento por Cabeçalhos (Header Routing)
- 🔄 Roteamento por Parâmetros de Consulta (Query Parameter Routing)
- ✅ Verificação de Saúde dos Destinos (Destination Health Checks)
- ⏳ TimeOuts Configuráveis
- 🔄 Transformações de Solicitação e Resposta
- 💾 Persistência de Sessão (Session Affinity)

---

## 🧩 Exemplo Básico do `service.json`

```jsonc
{
  // URL para autenticação do usuário
  "AuthOrigin": "http://localhost:3000/verify-user",

  // Políticas de Rate Limiting por serviço
  "RateLimit": {
    "serviceUserLimit": {
      // Limite de requisições permitidas
      "PermitLimit": 10,
      // Tipo da janela de tempo (horas)
      "WindowTypeTime": "HOURS",
      // Duração da janela
      "WindowTime": 2,
      // Ordem de processamento da fila
      "QueueProcessingOrder": "OLDEST_FIRST",
      // Tamanho máximo da fila (0 = sem fila)
      "QueueLimit": 0
    },
    "servicePostLimit": {
      "PermitLimit": 10,
      "WindowTypeTime": "HOURS",
      "WindowTime": 2,
      "QueueProcessingOrder": "OLDEST_FIRST",
      "QueueLimit": 0
    },
    "serviceTodoLimit": {
      "PermitLimit": 10,
      "WindowTypeTime": "HOURS",
      "WindowTime": 2,
      "QueueProcessingOrder": "OLDEST_FIRST",
      "QueueLimit": 0
    }
  },

  // Configuração do Proxy Reverso
  "ReverseProxy": {
    "Routes": {
      // Rota para o serviço de usuários
      "serviceUser": {
        // Nome do cluster
        "ClusterId": "serviceUser",
        // Política de autenticação aplicada
        "AuthorizationPolicy": "AuthPolicy",
        // Política de rate limit aplicada
        "RateLimiterPolicy": "serviceUserLimit",
        // Padrão de URL atendido
        "Match": {
          "Path": "/users/{**catch-all}"
        }
      },
      // Rota para o serviço de posts
      "servicePost": {
        "ClusterId": "servicePost",
        "AuthorizationPolicy": "AuthPolicy",
        "RateLimiterPolicy": "servicePostLimit",
        "Match": {
          "Path": "/posts/{**catch-all}"
        }
      },
      // Rota para o serviço de tarefas (todos)
      "serviceTodo": {
        "ClusterId": "serviceTodo",
        "AuthorizationPolicy": "AuthPolicy",
        "RateLimiterPolicy": "serviceTodoLimit",
        "Match": {
          "Path": "/todos/{**catch-all}"
        }
      }
    },

    // Definições dos clusters/destinos
    "Clusters": {
      "serviceUser": {
        "Destinations": {
          // Destino único para o cluster de usuários
          "origin1": {
            "Address": "https://service-user:5000/"
          }
        }
      },
      "servicePost": {
        "Destinations": {
          "origin1": {
            "Address": "https://service-post:5001/"
          }
        }
      },
      "serviceTodo": {
        // Política de balanceamento de carga
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          // Múltiplos destinos para balanceamento
          "origin1": {
            "Address": "https://service-todo1:5000/"
          },
          "origin2": {
            "Address": "https://service-todo2:5001/"
          }
        }
      }
    }
  }
}
```

---

## 🐳 IMAGEM

`isaiasdevback/box:0.0.1`

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
      - ./service.json:/app/service.json  # Monta o arquivo de config
    depends_on:
      - service-post
      - service-user
      - service-todo
      - service-auth
    environment:
      - SECRET=SUA_SECRET; # Informe uma secret para JWT, caso não informe será usada uma padrão;
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

  service-todo:
    image: seuusuario/service-cep:latest
    networks:
      - internal

  service-auth:
    image: seuusuario/service-auth:latest
    networks:
      - internal

networks:
  internal:
    driver: bridge
```

---

## ▶️ Como executar

1. Tenha o Docker instalado.
2. Crie um arquivo `service.json` com base no exemplo acima.
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

- O gateway valida o usuário chamando o endpoint que foi definido no serviço `AuthOrigin`.
- O serviço definido em `auth`, deve retornar um json que contenha a propriedade `{ "isAuthenticated": true or false }`, caso seja `true` ele gera um Bearer Token e autentica.
    
---

## 🛡️ Rate Limiting

- Limita o número de requisições por janela de tempo.
- Requisições acima do limite recebem código HTTP 429.

---

## ⚖️ Load Balancing

BOX por meio do YARP oferece diferentes estratégias de **balanceamento de carga** para distribuir as requisições entre múltiplos destinos. Abaixo estão os tipos disponíveis e suas descrições:


### 🔁 RoundRobin

- **Descrição**: Distribui as requisições de forma sequencial entre os destinos.
- **Uso comum**: Ideal para uma distribuição uniforme e previsível.
- **Observações**: Não considera o estado atual de carga de cada destino.
  

### 🎲 Random

- **Descrição**: Escolhe um destino aleatoriamente para cada requisição.
- **Uso comum**: Aplicações que podem tolerar variações na distribuição.
- **Observações**: Pode gerar distribuição desigual em ambientes com cargas assimétricas.
  

### 🧮 LeastRequests

- **Descrição**: Envia a requisição para o destino com menos requisições ativas.
- **Uso comum**: Quando é importante equilibrar ativamente a carga entre servidores.
- **Observações**: Precisa monitorar continuamente as requisições ativas.
  

### ⚡ PowerOfTwoChoices *(padrão)*

- **Descrição**: Escolhe dois destinos aleatórios e seleciona o que tem menos requisições ativas.
- **Uso comum**: Bom equilíbrio entre desempenho e distribuição eficiente.
- **Observações**: Mais leve que o `LeastRequests`, com resultado similar.
  

### 🔤 FirstAlphabetical

- **Descrição**: Seleciona o primeiro destino disponível com base em ordem alfabética.
- **Uso comum**: Ambientes com failover onde um destino é sempre preferido.
- **Observações**: Não deve ser usado para balanceamento com múltiplos destinos ativos.

### 💡 Exemplo de configuração

```json
{
  "ReverseProxy": {
    "Clusters": {
      "exampleCluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "destination1": { "Address": "https://example1.com/" },
          "destination2": { "Address": "https://example2.com/" }
        }
      }
    }
  }
}
```
---

## 🎩 Header Routing

Header Routing no YARP permite rotear solicitações com base nos cabeçalhos HTTP. Ao configurar rotas, você pode especificar que a correspondência deve ocorrer apenas se um cabeçalho específico estiver presente e tiver um valor correspondente. Isso é útil quando você deseja direcionar o tráfego com base em informações como tipo de usuário, versão da API ou qualquer outro critério enviado nos cabeçalhos da solicitação.[YARP (Yet Another Reverse Proxy) usando Routing HEADER](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/header-routing?view=aspnetcore-9.0) no Box Gateway.

---

## 🧭 Query Paramater Routing

Query Parameter Routing no YARP permite direcionar solicitações com base nos parâmetros de consulta da URL. Você pode configurar rotas para corresponder apenas quando um parâmetro de consulta específico estiver presente ou contiver um valor desejado. Isso permite flexibilidade na construção de rotas dinâmicas para diferentes cenários, como versões de API ou filtros de pesquisa. [YARP (Yet Another Reverse Proxy) Routing QUERY](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/queryparameter-routing?view=aspnetcore-9.0) no Box Gateway.

---

## 🖨 Request Transforms

É possivel modificar vários aspectos de uma solicitação HTTP, como caminho, parâmetros de consulta, método HTTP, cabeçalhos e mais, antes de encaminhá-la para o servidor de destino. Essas transformações incluem: 

- Modificar ou substituir o caminho da solicitação.
- Adicionar, remover ou substituir parâmetros de consulta.
- Alterar o método HTTP.
- Modificar ou adicionar cabeçalhos de solicitação.

[Transformando Requisições](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms-request?view=aspnetcore-9.0) no Box Gateway.

---

## 📡 Response Transforms

YARP detalha como modificar a resposta recebida do servidor de destino antes de retorná-la ao cliente. Isso inclui a possibilidade de alterar o corpo da resposta, cabeçalhos, status e outros aspectos. Essas transformações são úteis para ajustar o comportamento da resposta, como por exemplo, modificar o conteúdo para diferentes clientes ou redirecionar com base em certos critérios.

[Transformando Respostas](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms-response?view=aspnetcore-9.0) no Box Gateway.

---

## ⏰ Session Afinity 

A afinidade de sessão no YARP permite que as solicitações de um mesmo cliente sejam encaminhadas para o mesmo servidor backend durante a sessão. Isso é feito utilizando um identificador de sessão (geralmente via cookies), garantindo que o estado do cliente, como autenticação ou dados temporários, seja preservado entre as requisições. Essa técnica é fundamental em aplicações que precisam manter a consistência de dados entre diferentes interações.

[Session Afinity](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/session-affinity?view=aspnetcore-9.0) no Box Gateway.

---

## 💉 Health Check

O uso do Health check permite a configuração de checagens de saúde dos destinos para garantir que apenas servidores funcionais recebam tráfego. Através de verificações regulares, o YARP identifica se os destinos estão saudáveis. Se um servidor falhar na verificação, ele será temporariamente removido do balanceamento de carga até que se recupere. Essa abordagem melhora a confiabilidade da aplicação, evitando que usuários sejam direcionados para servidores com falhas.

[Health Check](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/dests-health-checks?view=aspnetcore-9.0) no Box Gateway.

---

## ⏰ Timeouts

YARP aborda a configuração de limites de tempo para conexões com servidores de destino, como tempos de espera para solicitações, leitura e gravação. Isso é importante para evitar que conexões lentas afetem a performance da aplicação, garantindo que o sistema continue responsivo. Você pode ajustar esses parâmetros para lidar com diferentes cenários de carga e melhorar a resiliência do serviço.

[Timeouts](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/timeouts?view=aspnetcore-9.0) no Box Gateway.

---

## ⚠️ Tratamento de Erros

- Todos os erros passam por middlewares centralizados
- Erros da API retornam mensagens padronizadas:
  
```jsonc
{
  "origin": "http://service.m1:5000",
  "endpoint": "/users",
  "statusCode": 500,
  "data": { /* resposta de erro do seu serviço */ }
}
```

---

## 🚩 IMPORTANTE !

As funcionlidades que possuem link externo para a documentação do YARP, é importante modificar apenas o JSON, tudo é feito pelo **service.json**.
Qualquer configuração deve ser adicionada nele.

---

## 🧪 Exemplo de Requisição

```bash
curl http://localhost:8080/users   -H "Authorization: Bearer SEU_TOKEN_JWT"
```

---

## 📌 Funcionalidades Futuras

Essas são as próximas melhorias previstas para o **BOX - API Gateway**:

- 🧰 **Cache Inteligente**  
  Implementação de cache para respostas frequentes, com configurações dinâmicas e invalidação automática.

---

## 📬 Contato

Desenvolvido por Isaías Vasconcelos.  
Sinta-se livre para abrir issues ou sugerir melhorias.

---

## 🧠 Por que BOX?

> Porque tudo cabe aqui dentro.  
> Centralize, controle e orquestre suas APIs com segurança e inteligência.
