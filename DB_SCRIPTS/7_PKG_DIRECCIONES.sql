-- ========================================
-- PAQUETE: PKG_DIRECCIONES
-- Gestión de direcciones y ciudades
-- ========================================

CREATE OR REPLACE PACKAGE pkg_direcciones AS

    -- Crea una nueva dirección de envío
    PROCEDURE crear_direccion(
        p_id_ciudad          IN direcciones.id_ciudad%TYPE,
        p_direccion_completa IN direcciones.direccion_completa%TYPE,
        p_barrio             IN direcciones.barrio%TYPE,
        p_codigo_postal      IN direcciones.codigo_postal%TYPE,
        p_referencia         IN direcciones.referencia%TYPE,
        p_id_direccion_out   OUT direcciones.id_direccion%TYPE
    );

    -- Actualiza una dirección existente
    PROCEDURE actualizar_direccion(
        p_id_direccion       IN direcciones.id_direccion%TYPE,
        p_id_ciudad          IN direcciones.id_ciudad%TYPE,
        p_direccion_completa IN direcciones.direccion_completa%TYPE,
        p_barrio             IN direcciones.barrio%TYPE,
        p_codigo_postal      IN direcciones.codigo_postal%TYPE,
        p_referencia         IN direcciones.referencia%TYPE
    );

    -- Obtiene el catálogo completo de ciudades
    PROCEDURE obtener_todas_ciudades(
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_direcciones;
/

CREATE OR REPLACE PACKAGE BODY pkg_direcciones AS

    PROCEDURE crear_direccion(
        p_id_ciudad          IN direcciones.id_ciudad%TYPE,
        p_direccion_completa IN direcciones.direccion_completa%TYPE,
        p_barrio             IN direcciones.barrio%TYPE,
        p_codigo_postal      IN direcciones.codigo_postal%TYPE,
        p_referencia         IN direcciones.referencia%TYPE,
        p_id_direccion_out   OUT direcciones.id_direccion%TYPE
    ) IS
        v_id_direccion direcciones.id_direccion%TYPE;
        v_existe_ciudad NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_existe_ciudad
        FROM ciudades
        WHERE id_ciudad = p_id_ciudad;

        IF v_existe_ciudad = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'La ciudad especificada no existe');
        END IF;

        v_id_direccion := seq_direcciones.NEXTVAL;

        INSERT INTO direcciones (
            id_direccion, 
            id_ciudad, 
            direccion_completa, 
            barrio, 
            codigo_postal, 
            referencia
        )
        VALUES (
            v_id_direccion, 
            p_id_ciudad, 
            p_direccion_completa, 
            p_barrio, 
            p_codigo_postal, 
            p_referencia
        );

        p_id_direccion_out := v_id_direccion;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20002, 'Error al crear dirección: ' || SQLERRM);
    END crear_direccion;

    PROCEDURE actualizar_direccion(
        p_id_direccion       IN direcciones.id_direccion%TYPE,
        p_id_ciudad          IN direcciones.id_ciudad%TYPE,
        p_direccion_completa IN direcciones.direccion_completa%TYPE,
        p_barrio             IN direcciones.barrio%TYPE,
        p_codigo_postal      IN direcciones.codigo_postal%TYPE,
        p_referencia         IN direcciones.referencia%TYPE
    ) IS
        v_existe_direccion NUMBER;
        v_existe_ciudad NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_existe_direccion
        FROM direcciones
        WHERE id_direccion = p_id_direccion;

        IF v_existe_direccion = 0 THEN
            RAISE_APPLICATION_ERROR(-20003, 'La dirección no existe');
        END IF;

        SELECT COUNT(*) INTO v_existe_ciudad
        FROM ciudades
        WHERE id_ciudad = p_id_ciudad;

        IF v_existe_ciudad = 0 THEN
            RAISE_APPLICATION_ERROR(-20004, 'La ciudad especificada no existe');
        END IF;

        UPDATE direcciones
        SET id_ciudad = p_id_ciudad,
            direccion_completa = p_direccion_completa,
            barrio = p_barrio,
            codigo_postal = p_codigo_postal,
            referencia = p_referencia
        WHERE id_direccion = p_id_direccion;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20005, 'Error al actualizar dirección: ' || SQLERRM);
    END actualizar_direccion;

    PROCEDURE obtener_todas_ciudades(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                id_ciudad,
                nombre,
                departamento,
                codigo_dane
            FROM ciudades
            ORDER BY departamento, nombre;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20007, 'Error al obtener ciudades: ' || SQLERRM);
    END obtener_todas_ciudades;

END pkg_direcciones;
/