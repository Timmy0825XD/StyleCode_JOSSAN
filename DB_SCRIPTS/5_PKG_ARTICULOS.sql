-- ========================================
-- PAQUETE: PKG_ARTICULOS
-- Gestión completa de artículos, variantes e imágenes
-- ========================================

CREATE OR REPLACE PACKAGE pkg_articulos AS

    -- Genera código SKU único basado en ID, talla y color
    PROCEDURE generar_codigo_sku(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_talla IN variantes_articulo.talla%TYPE,
        p_color IN variantes_articulo.color%TYPE,
        p_codigo_sku OUT VARCHAR2
    );

    -- Registra un nuevo artículo y retorna su ID
    PROCEDURE registrar_articulo(
        p_id_categoria_tipo IN articulos.id_categoria_tipo%TYPE,
        p_id_categoria_ocasion IN articulos.id_categoria_ocasion%TYPE,
        p_nombre IN articulos.nombre%TYPE,
        p_descripcion IN articulos.descripcion%TYPE,
        p_marca IN articulos.marca%TYPE,
        p_genero IN articulos.genero%TYPE,
        p_material IN articulos.material%TYPE,
        p_precio_base IN articulos.precio_base%TYPE,
        p_estado IN articulos.estado%TYPE,
        p_id_articulo_generado OUT NUMBER
    );

    -- Registra variante con SKU autogenerado
    PROCEDURE registrar_variante(
        p_id_articulo IN variantes_articulo.id_articulo%TYPE,
        p_talla IN variantes_articulo.talla%TYPE,
        p_color IN variantes_articulo.color%TYPE,
        p_stock IN variantes_articulo.stock%TYPE,
        p_id_variante_generada OUT NUMBER
    );

    -- Registra imagen asociada a un artículo
    PROCEDURE registrar_imagen(
        p_id_articulo IN imagenes_articulo.id_articulo%TYPE,
        p_url_imagen IN imagenes_articulo.url_imagen%TYPE,
        p_orden IN imagenes_articulo.orden%TYPE,
        p_es_principal IN imagenes_articulo.es_principal%TYPE,
        p_id_imagen_generada OUT NUMBER
    );

    -- Actualiza información general de un artículo
    PROCEDURE actualizar_articulo(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_nombre IN articulos.nombre%TYPE,
        p_descripcion IN articulos.descripcion%TYPE,
        p_marca IN articulos.marca%TYPE,
        p_genero IN articulos.genero%TYPE,
        p_material IN articulos.material%TYPE,
        p_precio_base IN articulos.precio_base%TYPE,
        p_estado IN articulos.estado%TYPE
    );

    -- Actualiza variante y regenera SKU
    PROCEDURE actualizar_variante(
        p_id_variante IN variantes_articulo.id_variante%TYPE,
        p_talla IN variantes_articulo.talla%TYPE,
        p_color IN variantes_articulo.color%TYPE,
        p_stock IN variantes_articulo.stock%TYPE,
        p_estado IN variantes_articulo.estado%TYPE
    );

    -- Inactiva variante si no está en pedidos activos
    PROCEDURE eliminar_variante(
        p_id_variante IN variantes_articulo.id_variante%TYPE
    );

    -- Actualiza imagen y gestiona imagen principal
    PROCEDURE actualizar_imagen(
        p_id_imagen IN imagenes_articulo.id_imagen%TYPE,
        p_url_imagen IN imagenes_articulo.url_imagen%TYPE,
        p_orden IN imagenes_articulo.orden%TYPE,
        p_es_principal IN imagenes_articulo.es_principal%TYPE
    );

    -- Elimina imagen si no es la única del artículo
    PROCEDURE eliminar_imagen(
        p_id_imagen IN imagenes_articulo.id_imagen%TYPE
    );

    -- Establece una imagen como principal para un artículo
    PROCEDURE establecer_imagen_principal(
        p_id_articulo IN imagenes_articulo.id_articulo%TYPE,
        p_id_imagen IN imagenes_articulo.id_imagen%TYPE
    );

    -- Lista todos los artículos activos con información resumida
    PROCEDURE listar_articulos_activos(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Obtiene detalle completo de un artículo (info + variantes + imágenes)
    PROCEDURE obtener_articulo_por_id(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_cursor_articulo OUT SYS_REFCURSOR,
        p_cursor_variantes OUT SYS_REFCURSOR,
        p_cursor_imagenes OUT SYS_REFCURSOR
    );

    -- Lista variantes activas de un artículo
    PROCEDURE listar_variantes_articulo(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Inactiva un artículo y todas sus variantes
    PROCEDURE eliminar_articulo(
        p_id_articulo IN articulos.id_articulo%TYPE
    );

    -- Obtiene catálogo de categorías por tipo
    PROCEDURE listar_categorias_tipo(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Obtiene catálogo de categorías por ocasión
    PROCEDURE listar_categorias_ocasion(
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_articulos;
/

CREATE OR REPLACE PACKAGE BODY pkg_articulos AS

    PROCEDURE generar_codigo_sku(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_talla IN variantes_articulo.talla%TYPE,
        p_color IN variantes_articulo.color%TYPE,
        p_codigo_sku OUT VARCHAR2
    ) IS
        v_talla_codigo VARCHAR2(3);
    BEGIN
        v_talla_codigo := UPPER(SUBSTR(p_talla, 1, 3));

        p_codigo_sku := LPAD(p_id_articulo, 4, '0') || 
                       v_talla_codigo || 
                       UPPER(SUBSTR(p_color, 1, 3));

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20001, 'Error al generar el código SKU: ' || SQLERRM);
    END generar_codigo_sku;

    PROCEDURE registrar_articulo(
        p_id_categoria_tipo IN articulos.id_categoria_tipo%TYPE,
        p_id_categoria_ocasion IN articulos.id_categoria_ocasion%TYPE,
        p_nombre IN articulos.nombre%TYPE,
        p_descripcion IN articulos.descripcion%TYPE,
        p_marca IN articulos.marca%TYPE,
        p_genero IN articulos.genero%TYPE,
        p_material IN articulos.material%TYPE,
        p_precio_base IN articulos.precio_base%TYPE,
        p_estado IN articulos.estado%TYPE,
        p_id_articulo_generado OUT NUMBER
    ) IS
    BEGIN
        p_id_articulo_generado := seq_articulos.NEXTVAL;

        INSERT INTO articulos (
            id_articulo, id_categoria_tipo, id_categoria_ocasion, nombre, descripcion,
            marca, genero, material, precio_base, estado, fecha_creacion
        ) VALUES (
            p_id_articulo_generado, p_id_categoria_tipo, p_id_categoria_ocasion, 
            p_nombre, p_descripcion, p_marca, p_genero, p_material, p_precio_base, 
            p_estado, SYSDATE
        );

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20002, 'Error al registrar artículo: ' || SQLERRM);
    END registrar_articulo;

    PROCEDURE registrar_variante(
        p_id_articulo IN variantes_articulo.id_articulo%TYPE,
        p_talla IN variantes_articulo.talla%TYPE,
        p_color IN variantes_articulo.color%TYPE,
        p_stock IN variantes_articulo.stock%TYPE,
        p_id_variante_generada OUT NUMBER
    ) IS
        v_codigo_sku VARCHAR2(10);
    BEGIN
        p_id_variante_generada := seq_variantes.NEXTVAL;

        generar_codigo_sku(p_id_articulo, p_talla, p_color, v_codigo_sku);

        INSERT INTO variantes_articulo (
            id_variante, id_articulo, talla, color, codigo_sku, stock, estado
        ) VALUES (
            p_id_variante_generada, p_id_articulo, p_talla, p_color, 
            v_codigo_sku, p_stock, 'A'
        );

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20004, 'Error al registrar variante: ' || SQLERRM);
    END registrar_variante;

    PROCEDURE registrar_imagen(
        p_id_articulo IN imagenes_articulo.id_articulo%TYPE,
        p_url_imagen IN imagenes_articulo.url_imagen%TYPE,
        p_orden IN imagenes_articulo.orden%TYPE,
        p_es_principal IN imagenes_articulo.es_principal%TYPE,
        p_id_imagen_generada OUT NUMBER
    ) IS
        v_es_principal CHAR(1);
    BEGIN
        p_id_imagen_generada := seq_imagenes.NEXTVAL;

        IF p_orden = 1 THEN
            v_es_principal := 'S';
        ELSE
            v_es_principal := 'N';
        END IF;

        INSERT INTO imagenes_articulo (
            id_imagen, id_articulo, url_imagen, orden, es_principal
        ) VALUES (
            p_id_imagen_generada, p_id_articulo, p_url_imagen, p_orden, v_es_principal
        );

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20005, 'Error al registrar imagen: ' || SQLERRM);
    END registrar_imagen;

    PROCEDURE actualizar_articulo(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_nombre IN articulos.nombre%TYPE,
        p_descripcion IN articulos.descripcion%TYPE,
        p_marca IN articulos.marca%TYPE,
        p_genero IN articulos.genero%TYPE,
        p_material IN articulos.material%TYPE,
        p_precio_base IN articulos.precio_base%TYPE,
        p_estado IN articulos.estado%TYPE
    ) IS
    BEGIN
        UPDATE articulos
        SET nombre = p_nombre,
            descripcion = p_descripcion,
            marca = p_marca,
            genero = p_genero,
            material = p_material,
            precio_base = p_precio_base,
            estado = p_estado
        WHERE id_articulo = p_id_articulo;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20008, 'No se encontró el artículo para actualizar.');
        END IF;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20009, 'Error al actualizar artículo: ' || SQLERRM);
    END actualizar_articulo;

    PROCEDURE actualizar_variante(
        p_id_variante IN variantes_articulo.id_variante%TYPE,
        p_talla IN variantes_articulo.talla%TYPE,
        p_color IN variantes_articulo.color%TYPE,
        p_stock IN variantes_articulo.stock%TYPE,
        p_estado IN variantes_articulo.estado%TYPE
    ) IS
        v_id_articulo NUMBER;
        v_codigo_sku VARCHAR2(10);
    BEGIN
        SELECT id_articulo INTO v_id_articulo
        FROM variantes_articulo
        WHERE id_variante = p_id_variante;

        generar_codigo_sku(v_id_articulo, p_talla, p_color, v_codigo_sku);

        UPDATE variantes_articulo
        SET talla = p_talla,
            color = p_color,
            codigo_sku = v_codigo_sku,
            stock = p_stock,
            estado = p_estado
        WHERE id_variante = p_id_variante;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20012, 'No se encontró la variante para actualizar.');
        END IF;

    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20013, 'La variante no existe.');
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20014, 'Error al actualizar variante: ' || SQLERRM);
    END actualizar_variante;

    PROCEDURE eliminar_variante(
        p_id_variante IN variantes_articulo.id_variante%TYPE
    ) IS
        v_en_pedidos NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_en_pedidos
        FROM detalles_pedido dp
        INNER JOIN pedidos p ON dp.id_pedido = p.id_pedido
        WHERE dp.id_variante = p_id_variante
          AND p.estado IN ('Pendiente', 'Confirmado', 'Enviado');

        IF v_en_pedidos > 0 THEN
            RAISE_APPLICATION_ERROR(-20015, 
                'No se puede eliminar la variante porque está en ' || 
                v_en_pedidos || ' pedido(s) activo(s).');
        END IF;

        UPDATE variantes_articulo
        SET estado = 'I'
        WHERE id_variante = p_id_variante;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20016, 'No se encontró la variante para eliminar.');
        END IF;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20017, 'Error al eliminar variante: ' || SQLERRM);
    END eliminar_variante;

    PROCEDURE actualizar_imagen(
        p_id_imagen IN imagenes_articulo.id_imagen%TYPE,
        p_url_imagen IN imagenes_articulo.url_imagen%TYPE,
        p_orden IN imagenes_articulo.orden%TYPE,
        p_es_principal IN imagenes_articulo.es_principal%TYPE
    ) IS
        v_id_articulo NUMBER;
    BEGIN
        SELECT id_articulo INTO v_id_articulo
        FROM imagenes_articulo
        WHERE id_imagen = p_id_imagen;

        IF p_es_principal = 'S' THEN
            UPDATE imagenes_articulo
            SET es_principal = 'N'
            WHERE id_articulo = v_id_articulo
              AND id_imagen != p_id_imagen;
        END IF;

        UPDATE imagenes_articulo
        SET url_imagen = p_url_imagen,
            orden = p_orden,
            es_principal = p_es_principal
        WHERE id_imagen = p_id_imagen;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20018, 'No se encontró la imagen para actualizar.');
        END IF;

    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20019, 'La imagen no existe.');
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20020, 'Error al actualizar imagen: ' || SQLERRM);
    END actualizar_imagen;

    PROCEDURE eliminar_imagen(
        p_id_imagen IN imagenes_articulo.id_imagen%TYPE
    ) IS
        v_id_articulo NUMBER;
        v_total_imagenes NUMBER;
        v_es_principal CHAR(1);
    BEGIN
        SELECT id_articulo, es_principal 
        INTO v_id_articulo, v_es_principal
        FROM imagenes_articulo
        WHERE id_imagen = p_id_imagen;

        SELECT COUNT(*) INTO v_total_imagenes
        FROM imagenes_articulo
        WHERE id_articulo = v_id_articulo;

        IF v_total_imagenes <= 1 THEN
            RAISE_APPLICATION_ERROR(-20021, 
                'No se puede eliminar la última imagen del artículo.');
        END IF;

        DELETE FROM imagenes_articulo
        WHERE id_imagen = p_id_imagen;

        IF v_es_principal = 'S' THEN
            UPDATE imagenes_articulo
            SET es_principal = 'S'
            WHERE id_articulo = v_id_articulo
              AND ROWNUM = 1
            ORDER BY orden;
        END IF;

    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20022, 'La imagen no existe.');
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20023, 'Error al eliminar imagen: ' || SQLERRM);
    END eliminar_imagen;

    PROCEDURE establecer_imagen_principal(
        p_id_articulo IN imagenes_articulo.id_articulo%TYPE,
        p_id_imagen IN imagenes_articulo.id_imagen%TYPE
    ) IS
        v_existe NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_existe
        FROM imagenes_articulo
        WHERE id_imagen = p_id_imagen
          AND id_articulo = p_id_articulo;

        IF v_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20024, 
                'La imagen no existe o no pertenece al artículo especificado.');
        END IF;

        UPDATE imagenes_articulo
        SET es_principal = 'N'
        WHERE id_articulo = p_id_articulo;

        UPDATE imagenes_articulo
        SET es_principal = 'S'
        WHERE id_imagen = p_id_imagen;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20025, 'Error al establecer imagen principal: ' || SQLERRM);
    END establecer_imagen_principal;

    PROCEDURE listar_articulos_activos(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT a.id_articulo,
                   a.nombre,
                   a.descripcion,
                   a.marca,
                   a.genero,
                   a.material,
                   a.precio_base,
                   a.estado,
                   a.fecha_creacion,
                   ct.nombre AS categoria_tipo,
                   co.nombre AS categoria_ocasion,
                   (SELECT COUNT(*) FROM variantes_articulo v WHERE v.id_articulo = a.id_articulo) AS total_variantes,
                   (SELECT url_imagen FROM imagenes_articulo i WHERE i.id_articulo = a.id_articulo AND i.es_principal = 'S' AND ROWNUM = 1) AS imagen_principal
            FROM articulos a
            INNER JOIN categorias_tipo ct ON a.id_categoria_tipo = ct.id_categoria_tipo
            INNER JOIN categorias_ocasion co ON a.id_categoria_ocasion = co.id_categoria_ocasion
            WHERE a.estado = 'A'
            ORDER BY a.fecha_creacion DESC;
    END listar_articulos_activos;

    PROCEDURE obtener_articulo_por_id(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_cursor_articulo OUT SYS_REFCURSOR,
        p_cursor_variantes OUT SYS_REFCURSOR,
        p_cursor_imagenes OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor_articulo FOR
            SELECT a.id_articulo,
                   a.nombre,
                   a.descripcion,
                   a.marca,
                   a.genero,
                   a.material,
                   a.precio_base,
                   a.estado,
                   a.fecha_creacion,
                   a.id_categoria_tipo,
                   a.id_categoria_ocasion,
                   ct.nombre AS categoria_tipo,
                   co.nombre AS categoria_ocasion
            FROM articulos a
            INNER JOIN categorias_tipo ct ON a.id_categoria_tipo = ct.id_categoria_tipo
            INNER JOIN categorias_ocasion co ON a.id_categoria_ocasion = co.id_categoria_ocasion
            WHERE a.id_articulo = p_id_articulo;

        OPEN p_cursor_variantes FOR
            SELECT id_variante,
                   talla,
                   color,
                   codigo_sku,
                   stock,
                   estado
            FROM variantes_articulo
            WHERE id_articulo = p_id_articulo
              AND estado = 'A';

        OPEN p_cursor_imagenes FOR
            SELECT id_imagen,
                   url_imagen,
                   orden,
                   es_principal
            FROM imagenes_articulo
            WHERE id_articulo = p_id_articulo
            ORDER BY orden;
    END obtener_articulo_por_id;

    PROCEDURE listar_variantes_articulo(
        p_id_articulo IN articulos.id_articulo%TYPE,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT id_variante,
                   talla,
                   color,
                   codigo_sku,
                   stock,
                   estado
            FROM variantes_articulo
            WHERE id_articulo = p_id_articulo
              AND estado = 'A';
    END listar_variantes_articulo;

    PROCEDURE eliminar_articulo(
        p_id_articulo IN articulos.id_articulo%TYPE
    ) IS
    BEGIN
        UPDATE articulos
        SET estado = 'I'
        WHERE id_articulo = p_id_articulo;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20010, 'El artículo no existe o ya está inactivo.');
        END IF;

        UPDATE variantes_articulo
        SET estado = 'I'
        WHERE id_articulo = p_id_articulo;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20011, 'Error al eliminar artículo: ' || SQLERRM);
    END eliminar_articulo;

    PROCEDURE listar_categorias_tipo(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT id_categoria_tipo,
                   nombre,
                   descripcion
            FROM categorias_tipo
            WHERE estado = 'A'
            ORDER BY nombre;
    END listar_categorias_tipo;

    PROCEDURE listar_categorias_ocasion(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT id_categoria_ocasion,
                   nombre,
                   descripcion
            FROM categorias_ocasion
            WHERE estado = 'A'
            ORDER BY nombre;
    END listar_categorias_ocasion;

END pkg_articulos;
/