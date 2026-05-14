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
(1, 1, 'SuperAdmin', 'super123', 1),
(2, 1, 'admin', 'admin123', 1),
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