USE AFReparosAutomotivos
GO

-- 20 Pessoas
INSERT INTO Pessoa (nome, celular, documento, tipo_doc)
VALUES
('Carlos Henrique Souza', '(19) 98765-4321', '123.456.789-01', 'F'),
('Mariana Lopes Almeida', '(19) 97654-3210', '234.567.890-12', 'F'),
('João Pedro Martins', '(19) 98888-7777', '345.678.901-23', 'F'),
('Ana Clara Ribeiro', '(19) 97777-6666', '456.789.012-34', 'F'),
('Lucas Gabriel Silva', '(19) 96666-5555', '567.890.123-45', 'F'),
('Fernanda Costa Lima', '(19) 95555-4444', '678.901.234-56', 'F'),
('Rafael Augusto Melo', '(19) 94444-3333', '789.012.345-67', 'F'),
('Patrícia Gomes Rocha', '(19) 93333-2222', '890.123.456-78', 'F'),
('Roberto Almeida', '(19) 92222-1111', '901.234.567-89', 'F'),
('Juliana Ferreira', '(19) 91111-0000', '012.345.678-90', 'F'),
('Empresa Auto Peças Silva', '(19) 3333-4444', '12.345.678/0001-99', 'J'),
('Marcelo Andrade', '(19) 90000-1111', '112.233.445-56', 'F'),
('Camila Santos', '(19) 88888-9999', '223.344.556-67', 'F'),
('Oficina Parceira LTDA', '(19) 3222-1111', '98.765.432/0001-88', 'J'),
('Eduardo Pereira', '(19) 77777-8888', '334.455.667-78', 'F'),
('Larissa Nogueira', '(19) 66666-7777', '445.566.778-89', 'F'),
('Thiago Barbosa', '(19) 55555-6666', '556.677.889-90', 'F'),
('Beatriz Oliveira', '(19) 44444-5555', '667.788.990-01', 'F'),
('Ricardo Mendes', '(19) 33333-4444', '778.899.001-12', 'F'),
('Natália Campos', '(19) 22222-3333', '889.900.112-23', 'F')
GO


-- Endereços das 20 Pessoas
INSERT INTO Endereco (pessoaId, logradouro, numero, cidade, estado, CEP)
VALUES
(1, 'Rua das Oficinas', '100', 'Campinas', 'SP', '13010-000'),
(2, 'Av. Brasil', '250', 'Campinas', 'SP', '13020-000'),
(3, 'Rua Mecânicos', '45', 'Campinas', 'SP', '13030-000'),
(4, 'Rua Pintores', '78', 'Campinas', 'SP', '13040-000'),
(5, 'Av. Automotiva', '300', 'Sumaré', 'SP', '13170-000'),
(6, 'Rua das Peças', '90', 'Hortolândia', 'SP', '13185-000'),
(7, 'Rua Funilaria', '120', 'Campinas', 'SP', '13050-000'),
(8, 'Av. Consulta', '500', 'Valinhos', 'SP', '13270-000'),
(9, 'Rua Cliente Um', '10', 'Campinas', 'SP', '13060-000'),
(10, 'Rua Cliente Dois', '20', 'Campinas', 'SP', '13061-000'),
(11, 'Av. Comercial', '300', 'Sumaré', 'SP', '13171-000'),
(12, 'Rua Azul', '45', 'Hortolândia', 'SP', '13186-000'),
(13, 'Rua Verde', '88', 'Valinhos', 'SP', '13271-000'),
(14, 'Av. Industrial', '900', 'Campinas', 'SP', '13062-000'),
(15, 'Rua Nova', '12', 'Paulínia', 'SP', '13140-000'),
(16, 'Rua Central', '34', 'Campinas', 'SP', '13063-000'),
(17, 'Av. Paulista', '150', 'Campinas', 'SP', '13064-000'),
(18, 'Rua das Flores', '55', 'Vinhedo', 'SP', '13280-000'),
(19, 'Rua América', '77', 'Campinas', 'SP', '13065-000'),
(20, 'Rua Primavera', '99', 'Sumaré', 'SP', '13172-000')
GO


-- Herança: Pessoas que são Funcionários
INSERT INTO Funcionario (idFuncionario, permissao, usuario, senha, statusFunc)
VALUES
(1, 1, 'SuperAdmin', '123', 1),
(2, 1, 'admin', '123456', 1),
(3, 2, 'joaopedro', 'joao123', 1),
(4, 2, 'anaclara', 'ana123', 1),
(5, 2, 'lucasgabriel', 'lucas123', 1),
(6, 2, 'fernandalima', 'fer123', 2),
(7, 2, 'rafaelmelo', 'rafael123', 2),
(8, 3, 'patriciarocha', 'pat123', 1)
GO

-- Herança: Pessoas que são Clientes
INSERT INTO Cliente (idCliente, email, statusCli, chaveCli)
VALUES
(9, 'roberto@email.com', 1, 'CLI-0001-0001-0001R'),
(10, 'juliana@email.com', 1, 'CLI-0002-0002-0002J'),
(11, 'contato@silvapecas.com', 1, 'CLI-0003-0003-0003E'),
(12, 'marcelo@email.com', 2, 'CLI-0004-0004-0004M'),
(13, 'camila@email.com', 1, 'CLI-0005-0005-0005C'),
(14, 'contato@parceira.com', 2, 'CLI-0006-0006-0006O'),
(15, 'eduardo@email.com', 1, 'CLI-0007-0007-0007E'),
(16, 'larissa@email.com', 1, 'CLI-0008-0008-0008L'),
(17, 'thiago@email.com', 2, 'CLI-0009-0009-0009T'),
(18, 'beatriz@email.com', 1, 'CLI-0010-0010-0010B'),
(19, 'ricardo@email.com', 1, 'CLI-0011-0011-0011R'),
(20, 'natalia@email.com', 1, 'CLI-0012-0012-0012N')
GO

-- Veículos necessários para os orçamentos
INSERT INTO Veiculo (clienteId, marca, placa, modelo, cor, ano)
VALUES
(9, 'Toyota', 'ABC1234', 'Corolla', 'Prata', 2022),
(9, 'Honda', 'DEF5678', 'Civic', 'Preto', 2021),
(9, 'Chevrolet', 'GHI9012', 'Onix', 'Branco', 2023),
(9, 'Volkswagen', 'JKL3456', 'Golf', 'Vermelho', 2019),
(9, 'Fiat', 'MNO7890', 'Argo', 'Cinza', 2022),
(10, 'Hyundai', 'PQR1234', 'HB20', 'Azul', 2020),
(12, 'Ford', 'STU5678', 'Ka', 'Branco', 2018),
(13, 'Jeep', 'VWX9012', 'Renegade', 'Verde', 2021),
(15, 'Nissan', 'YZA3456', 'Kicks', 'Cinza', 2022),
(18, 'Renault', 'BCD7890', 'Sandero', 'Prata', 2020);
GO


-- Orçamentos
INSERT INTO Orcamento(clienteId, funcionarioId, veiculoId, data_criacao, data_entrega, statusOrc, total, forma_pgto, parcelas)
VALUES
-- CLIENTE 9 
-- 3 pagos MAIS ANTIGOS
(9, 3, 1, '2025-01-10 09:00:00', '2025-01-15 17:00:00', 2, 1850.00, 'Cartão', 3),

(9, 4, 2, '2025-02-05 10:30:00', '2025-02-10 16:00:00', 2, 920.00, 'Pix', 1),

(9, 5, 3, '2025-03-12 14:00:00', '2025-03-18 11:00:00', 2, 3100.00, 'Boleto', 5),

-- 1 cancelado
(9, 3, 4, '2025-04-20 08:00:00', NULL, 3, 760.00, NULL, NULL),

-- 1 em aberto MAIS NOVO
(9, 4, 5, '2026-05-10 15:30:00', NULL, 1, 2450.00, NULL, NULL),


-- OUTROS CLIENTES
(10, 5, 6, '2025-06-01 09:15:00', NULL, 1, 1300.00, NULL, NULL),

(12, 3, 7, '2025-07-14 11:45:00', '2025-07-20 18:00:00', 2, 2780.00, 'Cartão', 4),

(13, 4, 8, '2025-08-03 13:20:00', NULL, 3, 990.00, NULL, NULL),

(15, 5, 9, '2025-09-18 10:00:00', '2025-09-22 17:00:00', 2, 4200.00, 'Pix', 1),

(18, 3, 10, '2026-01-25 16:10:00', NULL, 1, 1650.00, NULL, NULL)
GO

-- Serviços
INSERT INTO Servico (descricao, valorBase)
VALUES
('Funilaria Porta', 850.00),
('Pintura Completa', 2500.00),
('Troca de Para-choque', 1200.00),
('Polimento Técnico', 450.00),
('Alinhamento Estrutural', 1800.00),
('Troca de Farol', 600.00),
('Reparo de Lataria', 950.00),
('Cristalização', 400.00),
('Pintura de Capô', 700.00),
('Martelinho de Ouro', 550.00)
GO


-- 1 serviço distinto para cada orçamento
INSERT INTO Itens(orcamentoId, servicoId, funcionarioID, pecaId, preco, desconto, dataEntrega)
VALUES
(1, 1, 3, NULL, 850.00, 50.00, '2025-01-15 17:00:00'),
(2, 2, 4, NULL, 2500.00, 100.00, '2025-02-10 16:00:00'),
(3, 3, 5, NULL, 1200.00, NULL, '2025-03-18 11:00:00'),
(4, 4, 3, NULL, 450.00, NULL, NULL),
(5, 5, 4, NULL, 1800.00, 150.00, NULL),
(6, 6, 5, NULL, 600.00, NULL, NULL),
(7, 7, 3, NULL, 950.00, 75.00, '2025-07-20 18:00:00'),
(8, 8, 4, NULL, 400.00, NULL, NULL),
(9, 9, 5, NULL, 700.00, 50.00, '2025-09-22 17:00:00'),
(10, 10, 3, NULL, 550.00, NULL, NULL)
GO
