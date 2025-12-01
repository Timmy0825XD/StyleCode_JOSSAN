-- ========================================
-- CLOTHIX E-COMMERCE
-- Script de Estructura de Base de Datos
-- ========================================

-- ========================================
-- SECUENCIAS
-- ========================================

-- Catálogos
CREATE SEQUENCE seq_categorias_tipo START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_categorias_ocasion START WITH 1 INCREMENT BY 1;

-- Productos
CREATE SEQUENCE seq_articulos START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_variantes START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_imagenes START WITH 1 INCREMENT BY 1;

-- Geografía
CREATE SEQUENCE seq_ciudades START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_direcciones START WITH 1 INCREMENT BY 1;

-- Seguridad
CREATE SEQUENCE seq_roles START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_usuarios START WITH 1 INCREMENT BY 1;

-- Transacciones
CREATE SEQUENCE seq_pedidos START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_detalles_pedido START WITH 1 INCREMENT BY 1;

-- Facturación
CREATE SEQUENCE seq_facturas START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_detalles_factura START WITH 1 INCREMENT BY 1;

-- Promociones
CREATE SEQUENCE seq_cupones START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_cupones_usados START WITH 1 INCREMENT BY 1;

-- Operaciones
CREATE SEQUENCE seq_historial_precios START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_alertas_stock START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_favoritos START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_cola_emails START WITH 1 INCREMENT BY 1;

-- ========================================
-- TABLAS DE CATÁLOGO
-- ========================================

-- Categoriza productos por tipo (Camisa, Pantalón, etc)
CREATE TABLE categorias_tipo (
    id_categoria_tipo NUMBER NOT NULL,
    nombre VARCHAR2(20) NOT NULL,
    descripcion VARCHAR2(100),
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    fecha_creacion DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_categorias_tipo PRIMARY KEY (id_categoria_tipo),
    CONSTRAINT chk_cat_tipo_estado CHECK (estado IN ('A', 'I')),
    CONSTRAINT uk_cat_tipo_nombre UNIQUE (nombre)
);

-- Categoriza productos por ocasión de uso (Casual, Formal, Deportivo)
CREATE TABLE categorias_ocasion (
    id_categoria_ocasion NUMBER NOT NULL,
    nombre VARCHAR2(30) NOT NULL,
    descripcion VARCHAR2(100),
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    fecha_creacion DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_categorias_ocasion PRIMARY KEY (id_categoria_ocasion),
    CONSTRAINT chk_cat_ocasion_estado CHECK (estado IN ('A', 'I')),
    CONSTRAINT uk_cat_ocasion_nombre UNIQUE (nombre)
);

-- ========================================
-- TABLAS DE PRODUCTOS
-- ========================================

-- Artículos maestros (producto base sin variantes)
CREATE TABLE articulos (
    id_articulo NUMBER NOT NULL,
    id_categoria_tipo NUMBER NOT NULL,
    id_categoria_ocasion NUMBER NOT NULL,
    nombre VARCHAR2(30) NOT NULL,
    descripcion VARCHAR2(150),
    marca VARCHAR2(25) NOT NULL,
    genero VARCHAR2(10) NOT NULL,
    material VARCHAR2(25),
    precio_base NUMBER(6) NOT NULL,
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    fecha_creacion DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_articulos PRIMARY KEY (id_articulo),
    CONSTRAINT fk_art_cat_tipo FOREIGN KEY (id_categoria_tipo) 
        REFERENCES categorias_tipo(id_categoria_tipo),
    CONSTRAINT fk_art_cat_ocasion FOREIGN KEY (id_categoria_ocasion) 
        REFERENCES categorias_ocasion(id_categoria_ocasion),
    CONSTRAINT chk_art_estado CHECK (estado IN ('A', 'I')),
    CONSTRAINT chk_art_precio CHECK (precio_base > 0),
    CONSTRAINT chk_art_genero CHECK (genero IN ('Hombre', 'Mujer', 'Unisex', 'Niño', 'Niña'))
);

-- Variantes de artículos (combinaciones de talla/color con stock propio)
CREATE TABLE variantes_articulo (
    id_variante NUMBER NOT NULL,
    id_articulo NUMBER NOT NULL,
    talla VARCHAR2(3) NOT NULL,
    color VARCHAR2(15) NOT NULL,
    codigo_sku VARCHAR2(10) NOT NULL,
    stock NUMBER(3) DEFAULT 0 NOT NULL,
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    CONSTRAINT pk_variantes PRIMARY KEY (id_variante),
    CONSTRAINT fk_var_articulo FOREIGN KEY (id_articulo) 
        REFERENCES articulos(id_articulo) ON DELETE CASCADE,
    CONSTRAINT uk_var_sku UNIQUE (codigo_sku),
    CONSTRAINT uk_var_combinacion UNIQUE (id_articulo, talla, color),
    CONSTRAINT chk_var_estado CHECK (estado IN ('A', 'I')),
    CONSTRAINT chk_var_talla CHECK (
        talla IN ('XS', 'S', 'M', 'L', 'XL', 'XXL') 
        OR REGEXP_LIKE(talla, '^[0-9]{1,3}$')),
    CONSTRAINT chk_var_stock CHECK (stock >= 0)
);

-- Galería de imágenes por artículo
CREATE TABLE imagenes_articulo (
    id_imagen NUMBER NOT NULL,
    id_articulo NUMBER NOT NULL,
    url_imagen VARCHAR2(200) NOT NULL,
    orden NUMBER(2) DEFAULT 1 NOT NULL,
    es_principal CHAR(1) DEFAULT 'N' NOT NULL,
    CONSTRAINT pk_imagenes PRIMARY KEY (id_imagen),
    CONSTRAINT fk_img_articulo FOREIGN KEY (id_articulo) 
        REFERENCES articulos(id_articulo) ON DELETE CASCADE,
    CONSTRAINT chk_img_principal CHECK (es_principal IN ('S', 'N')),
    CONSTRAINT chk_img_orden CHECK (orden > 0)
);

-- ========================================
-- TABLAS DE GEOGRAFÍA Y ENVÍOS
-- ========================================

-- Ciudades disponibles para envío
CREATE TABLE ciudades (
    id_ciudad NUMBER NOT NULL,
    nombre VARCHAR2(20) NOT NULL,
    departamento VARCHAR2(20) NOT NULL,
    codigo_dane VARCHAR2(10),
    CONSTRAINT pk_ciudades PRIMARY KEY (id_ciudad),
    CONSTRAINT uk_ciudad_nombre UNIQUE (nombre, departamento)
);

-- Direcciones de envío de clientes
CREATE TABLE direcciones (
    id_direccion NUMBER NOT NULL,
    id_ciudad NUMBER NOT NULL,
    direccion_completa VARCHAR2(30) NOT NULL,
    barrio VARCHAR2(20) NOT NULL,
    codigo_postal VARCHAR2(8),
    referencia VARCHAR2(80),
    CONSTRAINT pk_direcciones PRIMARY KEY (id_direccion),
    CONSTRAINT fk_dir_ciudad FOREIGN KEY (id_ciudad) 
        REFERENCES ciudades(id_ciudad)
);

-- ========================================
-- TABLAS DE SEGURIDAD Y USUARIOS
-- ========================================

-- Roles del sistema (Cliente, Admin, etc)
CREATE TABLE roles (
    id_rol NUMBER NOT NULL,
    nombre VARCHAR2(10) NOT NULL,
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (id_rol),
    CONSTRAINT uk_rol_nombre UNIQUE (nombre),
    CONSTRAINT chk_rol_estado CHECK (estado IN ('A', 'I'))
);

-- Usuarios del sistema
CREATE TABLE usuarios (
    id_usuario NUMBER NOT NULL,
    id_rol NUMBER NOT NULL,
    id_direccion NUMBER,
    cedula VARCHAR2(10) NOT NULL,
    primer_nombre VARCHAR2(15) NOT NULL,
    segundo_nombre VARCHAR2(15),
    apellido_paterno VARCHAR2(15) NOT NULL,
    apellido_materno VARCHAR2(15),
    telefono_principal VARCHAR2(10) NOT NULL,
    telefono_secundario VARCHAR2(10),
    correo VARCHAR2(35) NOT NULL,
    contrasena VARCHAR2(35) NOT NULL,
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    fecha_registro DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_usuarios PRIMARY KEY (id_usuario),
    CONSTRAINT fk_usr_rol FOREIGN KEY (id_rol) 
        REFERENCES roles(id_rol),
    CONSTRAINT fk_usr_direccion FOREIGN KEY (id_direccion) 
        REFERENCES direcciones(id_direccion),
    CONSTRAINT uk_usr_cedula UNIQUE (cedula),
    CONSTRAINT uk_usr_correo UNIQUE (correo),
    CONSTRAINT chk_usr_estado CHECK (estado IN ('A', 'I'))
);

-- ========================================
-- TABLAS DE PROMOCIONES
-- ========================================

-- Cupones de descuento disponibles
CREATE TABLE cupones (
    id_cupon NUMBER NOT NULL,
    codigo VARCHAR2(20) NOT NULL,
    descripcion VARCHAR2(100) NOT NULL,
    tipo_descuento VARCHAR2(10) DEFAULT 'PORCENTAJE' NOT NULL,
    valor_descuento NUMBER(6,2) NOT NULL,
    usos_maximos NUMBER DEFAULT NULL, -- NULL = ilimitado
    usos_actuales NUMBER DEFAULT 0 NOT NULL,
    fecha_inicio DATE DEFAULT SYSDATE NOT NULL,
    fecha_expiracion DATE DEFAULT NULL, -- NULL = sin expiración
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    es_bienvenida CHAR(1) DEFAULT 'N' NOT NULL, -- Cupón automático para nuevos usuarios
    fecha_creacion DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_cupones PRIMARY KEY (id_cupon),
    CONSTRAINT uk_cupon_codigo UNIQUE (codigo),
    CONSTRAINT chk_cupon_tipo CHECK (tipo_descuento IN ('PORCENTAJE', 'MONTO')),
    CONSTRAINT chk_cupon_estado CHECK (estado IN ('A', 'I')),
    CONSTRAINT chk_cupon_bienvenida CHECK (es_bienvenida IN ('S', 'N')),
    CONSTRAINT chk_cupon_valor CHECK (valor_descuento > 0),
    CONSTRAINT chk_cupon_usos CHECK (usos_actuales >= 0)
);

-- Registro de cupones redimidos (un cupón por usuario)
CREATE TABLE cupones_usados (
    id_uso NUMBER NOT NULL,
    id_cupon NUMBER NOT NULL,
    id_usuario NUMBER NOT NULL,
    id_pedido NUMBER NOT NULL,
    descuento_aplicado NUMBER(8,2) NOT NULL,
    fecha_uso DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_cupones_usados PRIMARY KEY (id_uso),
    CONSTRAINT fk_uso_cupon FOREIGN KEY (id_cupon) 
        REFERENCES cupones(id_cupon),
    CONSTRAINT fk_uso_usuario FOREIGN KEY (id_usuario) 
        REFERENCES usuarios(id_usuario),
    CONSTRAINT uk_cupon_usuario UNIQUE (id_cupon, id_usuario),
    CONSTRAINT uk_cupon_pedido UNIQUE (id_pedido)
);

-- ========================================
-- TABLAS DE TRANSACCIONES
-- ========================================

-- Métodos de pago aceptados
CREATE TABLE metodos_pago (
    id_metodo NUMBER NOT NULL,
    nombre VARCHAR2(20) NOT NULL,
    estado CHAR(1) DEFAULT 'A' NOT NULL,
    CONSTRAINT pk_metodos_pago PRIMARY KEY (id_metodo),
    CONSTRAINT uk_metodo_nombre UNIQUE (nombre),
    CONSTRAINT chk_metodo_estado CHECK (estado IN ('A', 'I'))
);

-- Pedidos realizados por clientes
CREATE TABLE pedidos (
    id_pedido NUMBER NOT NULL,
    id_usuario NUMBER NOT NULL,
    id_direccion_envio NUMBER NOT NULL,
    id_metodo_pago NUMBER NOT NULL,
    id_cupon NUMBER DEFAULT NULL,
    numero_pedido VARCHAR2(14) NOT NULL,
    codigo_cupon VARCHAR2(20) DEFAULT NULL,
    fecha_pedido DATE DEFAULT SYSDATE NOT NULL,
    estado VARCHAR2(15) DEFAULT 'Pendiente' NOT NULL,
    subtotal NUMBER(8) NOT NULL,
    descuento_cupon NUMBER(8,2) DEFAULT 0 NOT NULL,
    costo_envio NUMBER(6) DEFAULT 0 NOT NULL,
    impuesto NUMBER(8) DEFAULT 0 NOT NULL,
    total NUMBER(8) NOT NULL,
    CONSTRAINT pk_pedidos PRIMARY KEY (id_pedido),
    CONSTRAINT fk_ped_usuario FOREIGN KEY (id_usuario) 
        REFERENCES usuarios(id_usuario),
    CONSTRAINT fk_ped_direccion FOREIGN KEY (id_direccion_envio) 
        REFERENCES direcciones(id_direccion),
    CONSTRAINT fk_ped_metodo FOREIGN KEY (id_metodo_pago) 
        REFERENCES metodos_pago(id_metodo),
    CONSTRAINT fk_ped_cupon FOREIGN KEY (id_cupon) 
        REFERENCES cupones(id_cupon),
    CONSTRAINT uk_ped_numero UNIQUE (numero_pedido),
    CONSTRAINT chk_ped_estado CHECK (estado IN ('Pendiente', 'Confirmado', 'Enviado', 'Entregado', 'Cancelado')),
    CONSTRAINT chk_ped_subtotal CHECK (subtotal >= 0),
    CONSTRAINT chk_ped_total CHECK (total >= 0)
);

-- Líneas de detalle de cada pedido
CREATE TABLE detalles_pedido (
    id_detalle NUMBER NOT NULL,
    id_pedido NUMBER NOT NULL,
    id_variante NUMBER NOT NULL,
    cantidad NUMBER(2) NOT NULL,
    precio_unitario NUMBER(6) NOT NULL,
    subtotal_linea NUMBER(8) NOT NULL,
    CONSTRAINT pk_detalles_pedido PRIMARY KEY (id_detalle),
    CONSTRAINT fk_det_pedido FOREIGN KEY (id_pedido) 
        REFERENCES pedidos(id_pedido) ON DELETE CASCADE,
    CONSTRAINT fk_det_variante FOREIGN KEY (id_variante) 
        REFERENCES variantes_articulo(id_variante),
    CONSTRAINT chk_det_cantidad CHECK (cantidad > 0),
    CONSTRAINT chk_det_precio CHECK (precio_unitario > 0),
    CONSTRAINT chk_det_subtotal CHECK (subtotal_linea >= 0)
);

-- ========================================
-- TABLAS DE FACTURACIÓN
-- ========================================

-- Facturas electrónicas generadas
CREATE TABLE facturas (
    id_factura NUMBER NOT NULL,
    id_pedido NUMBER NOT NULL,
    id_usuario NUMBER NOT NULL,
    numero_factura VARCHAR2(14) NOT NULL,
    cufe VARCHAR2(100), -- Código único de factura electrónica
    codigo_qr VARCHAR2(100),
    fecha_emision DATE DEFAULT SYSDATE NOT NULL,
    subtotal NUMBER(8) NOT NULL,
    impuesto NUMBER(8) DEFAULT 0 NOT NULL,
    total NUMBER(8) NOT NULL,
    estado VARCHAR2(15) DEFAULT 'Generada' NOT NULL,
    estado_dian VARCHAR2(15),
    fecha_dian DATE,
    CONSTRAINT pk_facturas PRIMARY KEY (id_factura),
    CONSTRAINT fk_fac_pedido FOREIGN KEY (id_pedido) 
        REFERENCES pedidos(id_pedido),
    CONSTRAINT fk_fac_usuario FOREIGN KEY (id_usuario) 
        REFERENCES usuarios(id_usuario),
    CONSTRAINT uk_fac_numero UNIQUE (numero_factura),
    CONSTRAINT uk_fac_cufe UNIQUE (cufe),
    CONSTRAINT chk_fac_estado CHECK (estado IN ('Generada', 'Enviada', 'Anulada')),
    CONSTRAINT chk_fac_dian CHECK (estado_dian IN ('Aprobada', 'Rechazada', 'Pendiente')),
    CONSTRAINT chk_fac_total CHECK (total >= 0)
);

-- Líneas de detalle de facturas
CREATE TABLE detalles_factura (
    id_detalle NUMBER NOT NULL,
    id_factura NUMBER NOT NULL,
    id_variante NUMBER NOT NULL,
    cantidad NUMBER(5) NOT NULL,
    precio_unitario NUMBER(6) NOT NULL,
    subtotal_linea NUMBER(8) NOT NULL,
    CONSTRAINT pk_detalles_factura PRIMARY KEY (id_detalle),
    CONSTRAINT fk_dfac_factura FOREIGN KEY (id_factura) 
        REFERENCES facturas(id_factura) ON DELETE CASCADE,
    CONSTRAINT fk_dfac_variante FOREIGN KEY (id_variante) 
        REFERENCES variantes_articulo(id_variante),
    CONSTRAINT chk_dfac_cantidad CHECK (cantidad > 0),
    CONSTRAINT chk_dfac_precio CHECK (precio_unitario > 0),
    CONSTRAINT chk_dfac_subtotal CHECK (subtotal_linea >= 0)
);

-- ========================================
-- TABLAS DE OPERACIONES
-- ========================================

-- Auditoría de cambios de precio
CREATE TABLE historial_precios (
    id_historial NUMBER NOT NULL,
    id_articulo NUMBER NOT NULL,
    precio_anterior NUMBER(6) NOT NULL,
    precio_nuevo NUMBER(6) NOT NULL,
    fecha_cambio DATE DEFAULT SYSDATE NOT NULL,
    usuario_modificador VARCHAR2(25),
    CONSTRAINT pk_historial_precios PRIMARY KEY (id_historial),
    CONSTRAINT fk_hist_articulo FOREIGN KEY (id_articulo) 
        REFERENCES articulos(id_articulo) ON DELETE CASCADE
);

-- Sistema de alertas por bajo stock
CREATE TABLE alertas_stock (
    id_alerta NUMBER NOT NULL,
    id_variante NUMBER NOT NULL,
    stock_actual NUMBER NOT NULL,
    fecha_alerta DATE DEFAULT SYSDATE NOT NULL,
    fecha_resolucion DATE,
    estado VARCHAR2(15) DEFAULT 'Pendiente' NOT NULL,
    resuelto_por VARCHAR2(20),
    CONSTRAINT pk_alertas_stock PRIMARY KEY (id_alerta),
    CONSTRAINT fk_alerta_variante FOREIGN KEY (id_variante) 
        REFERENCES variantes_articulo(id_variante) ON DELETE CASCADE,
    CONSTRAINT chk_alerta_estado CHECK (estado IN ('Pendiente', 'Resuelta'))
);

-- Lista de deseos de usuarios
CREATE TABLE favoritos (
    id_favorito NUMBER NOT NULL,
    id_usuario NUMBER NOT NULL,
    id_articulo NUMBER NOT NULL,
    fecha_agregado DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT pk_favoritos PRIMARY KEY (id_favorito),
    CONSTRAINT fk_fav_usuario FOREIGN KEY (id_usuario) 
        REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
    CONSTRAINT fk_fav_articulo FOREIGN KEY (id_articulo) 
        REFERENCES articulos(id_articulo) ON DELETE CASCADE,
    CONSTRAINT uk_fav_usuario_articulo UNIQUE (id_usuario, id_articulo)
);

-- Cola de emails pendientes de envío
CREATE TABLE cola_emails (
    id_email NUMBER NOT NULL,
    tipo_email VARCHAR2(20) NOT NULL,
    id_pedido NUMBER NOT NULL,
    destinatario VARCHAR2(35) NOT NULL,
    asunto VARCHAR2(50) NOT NULL,
    estado VARCHAR2(12) DEFAULT 'Pendiente' NOT NULL,
    intentos NUMBER DEFAULT 0 NOT NULL,
    fecha_creacion DATE DEFAULT SYSDATE NOT NULL,
    fecha_envio DATE,
    error_mensaje VARCHAR2(100),
    CONSTRAINT pk_cola_emails PRIMARY KEY (id_email),
    CONSTRAINT fk_email_pedido FOREIGN KEY (id_pedido) 
        REFERENCES pedidos(id_pedido) ON DELETE CASCADE,
    CONSTRAINT chk_email_estado CHECK (estado IN ('Pendiente', 'Enviado', 'Error'))
);

-- Agregar FK faltante para cupones_usados
ALTER TABLE cupones_usados ADD CONSTRAINT fk_uso_pedido 
    FOREIGN KEY (id_pedido) REFERENCES pedidos(id_pedido);

-- ========================================
-- ÍNDICES DE RENDIMIENTO
-- ========================================

-- Índices para búsquedas de productos
CREATE INDEX idx_art_nombre ON articulos(nombre);
CREATE INDEX idx_art_marca ON articulos(marca);
CREATE INDEX idx_art_genero ON articulos(genero);
CREATE INDEX idx_art_estado ON articulos(estado);
CREATE INDEX idx_art_cat_tipo ON articulos(id_categoria_tipo);
CREATE INDEX idx_art_cat_ocasion ON articulos(id_categoria_ocasion);

-- Índices para variantes y stock
CREATE INDEX idx_var_articulo ON variantes_articulo(id_articulo);
CREATE INDEX idx_var_talla ON variantes_articulo(talla);
CREATE INDEX idx_var_color ON variantes_articulo(color);
CREATE INDEX idx_var_stock ON variantes_articulo(stock);

-- Índices para usuarios
CREATE INDEX idx_usr_correo ON usuarios(correo);
CREATE INDEX idx_usr_cedula ON usuarios(cedula);
CREATE INDEX idx_usr_rol ON usuarios(id_rol);

-- Índices para pedidos
CREATE INDEX idx_ped_usuario ON pedidos(id_usuario);
CREATE INDEX idx_ped_estado ON pedidos(estado);
CREATE INDEX idx_ped_fecha ON pedidos(fecha_pedido);

-- Índices para facturas
CREATE INDEX idx_fac_pedido ON facturas(id_pedido);
CREATE INDEX idx_fac_usuario ON facturas(id_usuario);
CREATE INDEX idx_fac_estado ON facturas(estado);

-- Índices para alertas
CREATE INDEX idx_alertas_estado ON alertas_stock(estado);
CREATE INDEX idx_alertas_variante ON alertas_stock(id_variante);

-- Índices para favoritos
CREATE INDEX idx_fav_usuario ON favoritos(id_usuario);
CREATE INDEX idx_fav_articulo ON favoritos(id_articulo);
CREATE INDEX idx_fav_fecha ON favoritos(fecha_agregado);

-- Índices para emails
CREATE INDEX idx_cola_emails_estado ON cola_emails(estado);
CREATE INDEX idx_cola_emails_fecha ON cola_emails(fecha_creacion);

-- Índices para cupones
CREATE INDEX idx_cupones_codigo ON cupones(codigo);
CREATE INDEX idx_cupones_estado ON cupones(estado);
CREATE INDEX idx_cupones_bienvenida ON cupones(es_bienvenida);
CREATE INDEX idx_cupones_usados_usuario ON cupones_usados(id_usuario);
CREATE INDEX idx_cupones_usados_pedido ON cupones_usados(id_pedido);

-- ========================================
-- SINÓNIMOS PÚBLICOS - TABLAS
-- ========================================

CREATE PUBLIC SYNONYM categorias_tipo FOR clothix_admin.categorias_tipo;
CREATE PUBLIC SYNONYM categorias_ocasion FOR clothix_admin.categorias_ocasion;
CREATE PUBLIC SYNONYM articulos FOR clothix_admin.articulos;
CREATE PUBLIC SYNONYM variantes_articulo FOR clothix_admin.variantes_articulo;
CREATE PUBLIC SYNONYM imagenes_articulo FOR clothix_admin.imagenes_articulo;
CREATE PUBLIC SYNONYM ciudades FOR clothix_admin.ciudades;
CREATE PUBLIC SYNONYM direcciones FOR clothix_admin.direcciones;
CREATE PUBLIC SYNONYM roles FOR clothix_admin.roles;
CREATE PUBLIC SYNONYM usuarios FOR clothix_admin.usuarios;
CREATE PUBLIC SYNONYM metodos_pago FOR clothix_admin.metodos_pago;
CREATE PUBLIC SYNONYM pedidos FOR clothix_admin.pedidos;
CREATE PUBLIC SYNONYM detalles_pedido FOR clothix_admin.detalles_pedido;
CREATE PUBLIC SYNONYM facturas FOR clothix_admin.facturas;
CREATE PUBLIC SYNONYM detalles_factura FOR clothix_admin.detalles_factura;
CREATE PUBLIC SYNONYM historial_precios FOR clothix_admin.historial_precios;
CREATE PUBLIC SYNONYM alertas_stock FOR clothix_admin.alertas_stock;
CREATE PUBLIC SYNONYM favoritos FOR clothix_admin.favoritos;
CREATE PUBLIC SYNONYM cola_emails FOR clothix_admin.cola_emails;
CREATE PUBLIC SYNONYM cupones FOR clothix_admin.cupones;
CREATE PUBLIC SYNONYM cupones_usados FOR clothix_admin.cupones_usados;

-- ========================================
-- SINÓNIMOS PÚBLICOS - SECUENCIAS
-- ========================================

CREATE PUBLIC SYNONYM seq_categorias_tipo FOR clothix_admin.seq_categorias_tipo;
CREATE PUBLIC SYNONYM seq_categorias_ocasion FOR clothix_admin.seq_categorias_ocasion;
CREATE PUBLIC SYNONYM seq_articulos FOR clothix_admin.seq_articulos;
CREATE PUBLIC SYNONYM seq_variantes FOR clothix_admin.seq_variantes;
CREATE PUBLIC SYNONYM seq_imagenes FOR clothix_admin.seq_imagenes;
CREATE PUBLIC SYNONYM seq_ciudades FOR clothix_admin.seq_ciudades;
CREATE PUBLIC SYNONYM seq_direcciones FOR clothix_admin.seq_direcciones;
CREATE PUBLIC SYNONYM seq_roles FOR clothix_admin.seq_roles;
CREATE PUBLIC SYNONYM seq_usuarios FOR clothix_admin.seq_usuarios;
CREATE PUBLIC SYNONYM seq_pedidos FOR clothix_admin.seq_pedidos;
CREATE PUBLIC SYNONYM seq_detalles_pedido FOR clothix_admin.seq_detalles_pedido;
CREATE PUBLIC SYNONYM seq_facturas FOR clothix_admin.seq_facturas;
CREATE PUBLIC SYNONYM seq_detalles_factura FOR clothix_admin.seq_detalles_factura;
CREATE PUBLIC SYNONYM seq_historial_precios FOR clothix_admin.seq_historial_precios;
CREATE PUBLIC SYNONYM seq_alertas_stock FOR clothix_admin.seq_alertas_stock;
CREATE PUBLIC SYNONYM seq_favoritos FOR clothix_admin.seq_favoritos;
CREATE PUBLIC SYNONYM seq_cola_emails FOR clothix_admin.seq_cola_emails;
CREATE PUBLIC SYNONYM seq_cupones FOR clothix_admin.seq_cupones;
CREATE PUBLIC SYNONYM seq_cupones_usados FOR clothix_admin.seq_cupones_usados;

-- ========================================
-- SINÓNIMOS PÚBLICOS - PAQUETES
-- ========================================

CREATE PUBLIC SYNONYM pkg_direcciones FOR clothix_admin.pkg_direcciones;
CREATE PUBLIC SYNONYM pkg_usuarios FOR clothix_admin.pkg_usuarios;
CREATE PUBLIC SYNONYM pkg_articulos FOR clothix_admin.pkg_articulos;
CREATE PUBLIC SYNONYM pkg_pedidos FOR clothix_admin.pkg_pedidos;
CREATE PUBLIC SYNONYM pkg_facturacion FOR clothix_admin.pkg_facturacion;
CREATE PUBLIC SYNONYM pkg_emails FOR clothix_admin.pkg_emails;
CREATE PUBLIC SYNONYM pkg_alertas FOR clothix_admin.pkg_alertas;
CREATE PUBLIC SYNONYM pkg_favoritos FOR clothix_admin.pkg_favoritos;
CREATE PUBLIC SYNONYM pkg_estadisticas FOR clothix_admin.pkg_estadisticas;
CREATE PUBLIC SYNONYM pkg_cupones FOR clothix_admin.pkg_cupones;
CREATE PUBLIC SYNONYM pkg_reportes FOR clothix_admin.pkg_reportes;

-- ========================================
-- FIN DEL SCRIPT
-- ========================================