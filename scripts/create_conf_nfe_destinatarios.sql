-- Cria tabela de destinatários por empresa
-- Execute no banco do cliente para habilitar mapeamento de destinatários

CREATE TABLE IF NOT EXISTS conf_nfe_destinatarios (
	id SERIAL PRIMARY KEY,
	codigo_empresa INTEGER NOT NULL,
	destinatario_email TEXT NOT NULL,
	descricao TEXT,
	ativo BOOLEAN DEFAULT TRUE,
	criado_em TIMESTAMP WITHOUT TIME ZONE DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_conf_nfe_destinatarios_codigo_empresa ON conf_nfe_destinatarios(codigo_empresa);
