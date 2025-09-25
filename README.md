# RAGFlow to OpenAI 代理

简单的反向代理服务，将 RAGFlow API 转换为 OpenAI 兼容格式。

## 快速开始

### 拉取镜像
```bash
docker pull docker.nisedt.cn/ragflow2openai:latest
```
### 运行容器
```bash
docker run -d --name rag -p 8080:8080 -e TARGET_BASE_URL="http://your-ragflow-server.com" docker.nisedt.cn/ragflow2openai:latest
```
## 配置

### 环境变量

| 环境变量 | 说明 | 示例 |
|---------|------|------|
| `TARGET_BASE_URL` | RAGFlow 服务器地址 | `http://your-server.com` |

### 端口

- 容器端口: `8080`
- 建议映射: `8080:8080`