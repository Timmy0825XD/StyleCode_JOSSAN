-- ========================================
-- PAQUETE: PKG_EMAILS
-- Gestión de cola de emails y notificaciones
-- ========================================

CREATE OR REPLACE PACKAGE pkg_emails AS

    -- Obtiene emails pendientes de envío (máximo 3 intentos)
    PROCEDURE obtener_emails_pendientes(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Marca un email como enviado exitosamente
    PROCEDURE marcar_email_enviado(
        p_id_email IN NUMBER
    );

    -- Registra error en envío e incrementa contador de intentos
    PROCEDURE marcar_email_error(
        p_id_email IN NUMBER, 
        p_error_mensaje IN VARCHAR2
    );

    -- Obtiene datos completos de pedido para email de confirmación
    PROCEDURE obtener_datos_email_pedido(
        p_id_pedido IN NUMBER,
        p_cursor_pedido OUT SYS_REFCURSOR,
        p_cursor_productos OUT SYS_REFCURSOR
    );

    -- Envía cupón promocional a todos los clientes activos
    PROCEDURE enviar_email_cupon_masivo(
        p_id_cupon IN NUMBER
    );

    -- Obtiene datos del cupón de bienvenida para nuevo usuario
    PROCEDURE obtener_datos_cupon_bienvenida(
        p_correo IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_emails;
/

CREATE OR REPLACE PACKAGE BODY pkg_emails AS

    PROCEDURE obtener_emails_pendientes(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT id_email, tipo_email, id_pedido, destinatario, asunto, intentos, fecha_creacion
            FROM cola_emails
            WHERE estado = 'Pendiente' AND intentos < 3
            ORDER BY fecha_creacion ASC;
    END obtener_emails_pendientes;

    PROCEDURE marcar_email_enviado(
        p_id_email IN NUMBER
    ) IS
    BEGIN
        UPDATE cola_emails
        SET estado = 'Enviado', fecha_envio = SYSDATE, error_mensaje = NULL
        WHERE id_email = p_id_email;
        COMMIT;
    END marcar_email_enviado;

    PROCEDURE marcar_email_error(
        p_id_email IN NUMBER, 
        p_error_mensaje IN VARCHAR2
    ) IS
    BEGIN
        UPDATE cola_emails
        SET intentos = intentos + 1,
            error_mensaje = SUBSTR(p_error_mensaje, 1, 500),
            estado = CASE WHEN intentos >= 2 THEN 'Error' ELSE 'Pendiente' END
        WHERE id_email = p_id_email;
        COMMIT;
    END marcar_email_error;

    PROCEDURE obtener_datos_email_pedido(
        p_id_pedido IN NUMBER,
        p_cursor_pedido OUT SYS_REFCURSOR,
        p_cursor_productos OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor_pedido FOR
            SELECT p.numero_pedido, p.fecha_pedido, p.estado, p.subtotal, p.costo_envio,
                   p.impuesto, p.total, u.primer_nombre || ' ' || u.apellido_paterno AS nombre_cliente,
                   u.correo, d.direccion_completa, d.barrio, c.nombre AS ciudad,
                   c.departamento, mp.nombre AS metodo_pago
            FROM pedidos p
            INNER JOIN usuarios u ON p.id_usuario = u.id_usuario
            INNER JOIN direcciones d ON p.id_direccion_envio = d.id_direccion
            INNER JOIN ciudades c ON d.id_ciudad = c.id_ciudad
            INNER JOIN metodos_pago mp ON p.id_metodo_pago = mp.id_metodo
            WHERE p.id_pedido = p_id_pedido;

        OPEN p_cursor_productos FOR
            SELECT a.nombre AS nombre_producto, a.marca, v.talla, v.color,
                   dp.cantidad, dp.precio_unitario, dp.subtotal_linea, img.url_imagen
            FROM detalles_pedido dp
            INNER JOIN variantes_articulo v ON dp.id_variante = v.id_variante
            INNER JOIN articulos a ON v.id_articulo = a.id_articulo
            LEFT JOIN (SELECT id_articulo, url_imagen FROM imagenes_articulo 
                       WHERE es_principal = 'S' AND ROWNUM = 1) img ON a.id_articulo = img.id_articulo
            WHERE dp.id_pedido = p_id_pedido;
    END obtener_datos_email_pedido;

    PROCEDURE enviar_email_cupon_masivo(
        p_id_cupon IN NUMBER
    ) IS
        v_codigo_cupon VARCHAR2(20);
    BEGIN
        SELECT codigo INTO v_codigo_cupon
        FROM cupones
        WHERE id_cupon = p_id_cupon;

        INSERT INTO cola_emails (
            id_email,
            tipo_email,
            id_pedido,
            destinatario,
            asunto,
            estado
        )
        SELECT 
            seq_cola_emails.NEXTVAL,
            'CUPON_NUEVO',
            p_id_cupon,
            u.correo,
            '¡Nuevo cupón disponible! ' || v_codigo_cupon || ' 🎁',
            'Pendiente'
        FROM usuarios u
        WHERE u.id_rol = 2
        AND u.estado = 'A'
        AND NOT EXISTS (
            SELECT 1 FROM cupones_usados cu
            WHERE cu.id_cupon = p_id_cupon
            AND cu.id_usuario = u.id_usuario
        );

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al enviar emails masivos: ' || SQLERRM);
    END enviar_email_cupon_masivo;

    PROCEDURE obtener_datos_cupon_bienvenida(
        p_correo IN VARCHAR2,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                u.primer_nombre || ' ' || u.apellido_paterno AS nombre_cliente,
                c.codigo,
                c.descripcion,
                c.tipo_descuento,
                c.valor_descuento,
                c.fecha_expiracion
            FROM usuarios u
            CROSS JOIN cupones c
            WHERE u.correo = p_correo
            AND c.es_bienvenida = 'S'
            AND c.estado = 'A'
            AND ROWNUM = 1;
    END obtener_datos_cupon_bienvenida;

END pkg_emails;
/