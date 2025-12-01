-- ========================================
-- PAQUETE: PKG_FAVORITOS
-- Gestión de lista de deseos de usuarios
-- ========================================

CREATE OR REPLACE PACKAGE pkg_favoritos AS

    -- Elimina un artículo de favoritos
    PROCEDURE eliminar_favorito(
        p_id_usuario IN NUMBER,
        p_id_articulo IN NUMBER
    );

    -- Agrega o quita artículo de favoritos (toggle)
    PROCEDURE toggle_favorito(
        p_id_usuario IN NUMBER,
        p_id_articulo IN NUMBER,
        p_agregado OUT NUMBER
    );

    -- Obtiene lista completa de favoritos de un usuario
    PROCEDURE obtener_favoritos_usuario(
        p_id_usuario IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_favoritos;
/

CREATE OR REPLACE PACKAGE BODY pkg_favoritos AS

    PROCEDURE eliminar_favorito(
        p_id_usuario IN NUMBER,
        p_id_articulo IN NUMBER
    ) IS
        v_existe NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_existe
        FROM favoritos
        WHERE id_usuario = p_id_usuario
          AND id_articulo = p_id_articulo;

        IF v_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20410, 'Este artículo no está en favoritos');
        END IF;

        DELETE FROM favoritos
        WHERE id_usuario = p_id_usuario
          AND id_articulo = p_id_articulo;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20499, 'Error al eliminar favorito: ' || SQLERRM);
    END eliminar_favorito;

    PROCEDURE toggle_favorito(
        p_id_usuario IN NUMBER,
        p_id_articulo IN NUMBER,
        p_agregado OUT NUMBER
    ) IS
        v_existe NUMBER;
        v_id_favorito NUMBER;
        v_estado_articulo CHAR(1);
    BEGIN
        SELECT COUNT(*) INTO v_existe
        FROM usuarios
        WHERE id_usuario = p_id_usuario AND estado = 'A';

        IF v_existe = 0 THEN
            RAISE_APPLICATION_ERROR(-20420, 'Usuario no encontrado o inactivo');
        END IF;

        SELECT COUNT(*) INTO v_existe
        FROM favoritos
        WHERE id_usuario = p_id_usuario
          AND id_articulo = p_id_articulo;

        IF v_existe > 0 THEN
            DELETE FROM favoritos
            WHERE id_usuario = p_id_usuario
              AND id_articulo = p_id_articulo;

            p_agregado := 0;
        ELSE
            BEGIN
                SELECT estado INTO v_estado_articulo
                FROM articulos
                WHERE id_articulo = p_id_articulo;

                IF v_estado_articulo = 'I' THEN
                    RAISE_APPLICATION_ERROR(-20421, 'No se puede agregar a favoritos un artículo inactivo');
                END IF;
            EXCEPTION
                WHEN NO_DATA_FOUND THEN
                    RAISE_APPLICATION_ERROR(-20422, 'Artículo no encontrado');
            END;

            SELECT seq_favoritos.NEXTVAL INTO v_id_favorito FROM DUAL;

            INSERT INTO favoritos (
                id_favorito,
                id_usuario,
                id_articulo,
                fecha_agregado
            ) VALUES (
                v_id_favorito,
                p_id_usuario,
                p_id_articulo,
                SYSDATE
            );

            p_agregado := 1;
        END IF;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20499, 'Error al alternar favorito: ' || SQLERRM);
    END toggle_favorito;

    PROCEDURE obtener_favoritos_usuario(
        p_id_usuario IN NUMBER,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                f.id_favorito,
                f.id_articulo,
                f.fecha_agregado,
                a.nombre,
                a.descripcion,
                a.marca,
                a.genero,
                a.material,
                a.precio_base,
                a.estado,
                ct.nombre AS categoria_tipo,
                co.nombre AS categoria_ocasion,
                (SELECT url_imagen 
                 FROM imagenes_articulo 
                 WHERE id_articulo = a.id_articulo 
                   AND es_principal = 'S' 
                   AND ROWNUM = 1) AS imagen_principal,
                (SELECT NVL(SUM(stock), 0)
                 FROM variantes_articulo
                 WHERE id_articulo = a.id_articulo
                   AND estado = 'A') AS stock_total,
                (SELECT COUNT(*)
                 FROM variantes_articulo
                 WHERE id_articulo = a.id_articulo
                   AND estado = 'A'
                   AND stock > 0) AS variantes_disponibles
            FROM favoritos f
            INNER JOIN articulos a ON f.id_articulo = a.id_articulo
            INNER JOIN categorias_tipo ct ON a.id_categoria_tipo = ct.id_categoria_tipo
            INNER JOIN categorias_ocasion co ON a.id_categoria_ocasion = co.id_categoria_ocasion
            WHERE f.id_usuario = p_id_usuario
              AND a.estado = 'A'
            ORDER BY f.fecha_agregado DESC;
    END obtener_favoritos_usuario;

END pkg_favoritos;
/