# DesktopAgent

Breve: aplicação que captura a tela principal, envia a imagem para o modelo (Gemini) para decidir um clique e executa o clique no sistema.

Principais pontos
- A imagem enviada pode ser comprimida (apenas qualidade JPEG) sem alterar largura/altura.
- Versão da imagem enviada é salva em disco (por padrão em `Analizer\bin\Debug`).
- As credenciais sensíveis devem vir de variáveis de ambiente ou _user secrets_ — não commitá-las.

Variáveis de ambiente usadas
- `GEMINI_API_KEY` — chave do Gemini (obrigatório para chamadas).
- `GEMINI_API_URL` — URL da API (opcional; padrão no código usa v1beta).
- `RETRY_IA_CONNECTION` — número de tentativas em caso de 429/503 (padrão: `3`).
- `MAX_IMAGE_KB` — se > 0, tenta comprimir a imagem por qualidade até esse limite (KB). Se `0` ou ausente, não comprime.
- `SENT_IMAGES_DIR` — diretório onde a imagem enviada é salva. Padrão usado no código: `Analizer\bin\Debug`.
- `OPENAI_API_KEY`, `OPENAI_API_URL` — se aplicável em outras integrações.

Exemplo seguro de `launchSettings.json` (use placeholders, NÃO comite chaves reais)