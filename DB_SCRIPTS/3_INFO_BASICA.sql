-- ========================================
-- CLOTHIX E-COMMERCE
-- Datos Básicos e Iniciales
-- ========================================

-- ========================================
-- CATEGORÍAS POR TIPO DE PRENDA
-- ========================================

INSERT INTO categorias_tipo (id_categoria_tipo, nombre, descripcion)
VALUES (seq_categorias_tipo.NEXTVAL, 'Superiores', 'Camisetas, camisas, blusas, suéteres, chaquetas');

INSERT INTO categorias_tipo (id_categoria_tipo, nombre, descripcion)
VALUES (seq_categorias_tipo.NEXTVAL, 'Inferiores', 'Pantalones, faldas, shorts, leggins');

INSERT INTO categorias_tipo (id_categoria_tipo, nombre, descripcion)
VALUES (seq_categorias_tipo.NEXTVAL, 'Conjuntos', 'Trajes, pijamas, uniformes');

INSERT INTO categorias_tipo (id_categoria_tipo, nombre, descripcion)
VALUES (seq_categorias_tipo.NEXTVAL, 'Exteriores', 'Abrigos, chaquetas, impermeables');

INSERT INTO categorias_tipo (id_categoria_tipo, nombre, descripcion)
VALUES (seq_categorias_tipo.NEXTVAL, 'Interiores', 'Ropa interior, camisetas interiores, calcetines');

-- ========================================
-- CATEGORÍAS POR OCASIÓN
-- ========================================

INSERT INTO categorias_ocasion (id_categoria_ocasion, nombre, descripcion)
VALUES (seq_categorias_ocasion.NEXTVAL, 'Casual', 'Ropa para uso diario, informal');

INSERT INTO categorias_ocasion (id_categoria_ocasion, nombre, descripcion)
VALUES (seq_categorias_ocasion.NEXTVAL, 'Formal / Oficina', 'Ropa para trabajo o eventos formales');

INSERT INTO categorias_ocasion (id_categoria_ocasion, nombre, descripcion)
VALUES (seq_categorias_ocasion.NEXTVAL, 'Deportiva', 'Ropa para actividades deportivas');

INSERT INTO categorias_ocasion (id_categoria_ocasion, nombre, descripcion)
VALUES (seq_categorias_ocasion.NEXTVAL, 'Fiesta o Gala', 'Ropa para eventos nocturnos, fiestas o ceremonias');

INSERT INTO categorias_ocasion (id_categoria_ocasion, nombre, descripcion)
VALUES (seq_categorias_ocasion.NEXTVAL, 'De Playa', 'Ropa para vacaciones, playa o piscina');

INSERT INTO categorias_ocasion (id_categoria_ocasion, nombre, descripcion)
VALUES (seq_categorias_ocasion.NEXTVAL, 'De Descanso / Pijamas', 'Ropa para dormir o descanso');

INSERT INTO categorias_ocasion (id_categoria_ocasion, nombre, descripcion)
VALUES (seq_categorias_ocasion.NEXTVAL, 'Escolar / Institucional', 'Uniformes escolares o institucionales');

-- ========================================
-- ROLES DEL SISTEMA
-- ========================================

INSERT INTO roles (id_rol, nombre) VALUES (seq_roles.NEXTVAL, 'ADMIN');
INSERT INTO roles (id_rol, nombre) VALUES (seq_roles.NEXTVAL, 'CLIENTE');

-- ========================================
-- MÉTODOS DE PAGO
-- Códigos según normativa DIAN
-- ========================================

INSERT INTO metodos_pago (id_metodo, nombre) VALUES (1, 'Medio no definido');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (10, 'Efectivo');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (20, 'Cheque');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (42, 'Consignación');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (47, 'Transferencia');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (48, 'Tarjeta Crédito');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (49, 'Tarjeta Débito');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (71, 'Bonos');
INSERT INTO metodos_pago (id_metodo, nombre) VALUES (72, 'Vales');

-- ========================================
-- USUARIO ADMINISTRADOR
-- ========================================

INSERT INTO usuarios (
    id_usuario, 
    id_rol, 
    id_direccion, 
    cedula, 
    primer_nombre, 
    segundo_nombre, 
    apellido_paterno, 
    apellido_materno, 
    telefono_principal, 
    telefono_secundario, 
    correo, 
    contrasena
) VALUES (
    seq_usuarios.NEXTVAL,
    1,
    NULL,
    '1000000001',
    'Admin',
    NULL,
    'Principal',
    NULL,
    '3100000000',
    NULL,
    'admin@tienda.com',
    'admin123'
);

-- ========================================
-- CUPONES DE BIENVENIDA
-- ========================================

INSERT INTO cupones (
    id_cupon,
    codigo,
    descripcion,
    tipo_descuento,
    valor_descuento,
    usos_maximos,
    es_bienvenida,
    estado
) VALUES (
    seq_cupones.NEXTVAL,
    'BIENVENIDA10',
    '¡Bienvenido! 10% de descuento en tu primera compra',
    'PORCENTAJE',
    10.00,
    NULL,
    'S',
    'A'
);

-- ========================================
-- CIUDADES DE COLOMBIA
-- ========================================

-- Capitales departamentales
INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Bogotá', 'Cundinamarca', '11001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Medellín', 'Antioquia', '05001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Cali', 'Valle del Cauca', '76001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Barranquilla', 'Atlántico', '08001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Cartagena', 'Bolívar', '13001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Bucaramanga', 'Santander', '68001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Pereira', 'Risaralda', '66001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Manizales', 'Caldas', '17001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Ibagué', 'Tolima', '73001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Villavicencio', 'Meta', '50001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Neiva', 'Huila', '41001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Armenia', 'Quindío', '63001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Cúcuta', 'Norte de Santander', '54001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Santa Marta', 'Magdalena', '47001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Sincelejo', 'Sucre', '70001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Valledupar', 'Cesar', '20001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Popayán', 'Cauca', '19001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Tunja', 'Boyacá', '15001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Florencia', 'Caquetá', '18001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Mocoa', 'Putumayo', '86001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Quibdó', 'Chocó', '27001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Riohacha', 'La Guajira', '44001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Yopal', 'Casanare', '85001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Montería', 'Córdoba', '23001');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Pasto', 'Nariño', '52001');

-- Ciudades principales del área metropolitana
INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Soledad', 'Atlántico', '08078');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Envigado', 'Antioquia', '05032');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Bello', 'Antioquia', '05088');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Itagüí', 'Antioquia', '05035');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Palmira', 'Valle del Cauca', '76616');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Buenaventura', 'Valle del Cauca', '76109');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Fusagasugá', 'Cundinamarca', '25200');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Soacha', 'Cundinamarca', '25754');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Rionegro', 'Antioquia', '05697');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Tuluá', 'Valle del Cauca', '76823');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Cartago', 'Valle del Cauca', '76130');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Apartadó', 'Antioquia', '05045');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Barrancabermeja', 'Santander', '68081');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Floridablanca', 'Santander', '68077');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Sogamoso', 'Boyacá', '15753');

INSERT INTO ciudades (id_ciudad, nombre, departamento, codigo_dane) 
VALUES (seq_ciudades.NEXTVAL, 'Duitama', 'Boyacá', '15022');

COMMIT;