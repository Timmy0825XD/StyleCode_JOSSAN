-- ========================================
-- PAQUETE: PKG_USUARIOS
-- Gestión de usuarios y autenticación
-- ========================================

CREATE OR REPLACE PACKAGE pkg_usuarios AS

    -- Valida credenciales y retorna información del usuario
    PROCEDURE login_usuario(
        p_correo IN usuarios.correo%TYPE,
        p_contrasena IN usuarios.contrasena%TYPE,
        p_cursor OUT SYS_REFCURSOR
    );

    -- Registra un nuevo usuario con rol de cliente
    PROCEDURE crear_usuario(
        p_id_direccion IN usuarios.id_direccion%TYPE,
        p_cedula IN usuarios.cedula%TYPE,
        p_primer_nombre IN usuarios.primer_nombre%TYPE,
        p_segundo_nombre IN usuarios.segundo_nombre%TYPE,
        p_apellido_paterno IN usuarios.apellido_paterno%TYPE,
        p_apellido_materno IN usuarios.apellido_materno%TYPE,
        p_telefono_principal IN usuarios.telefono_principal%TYPE,
        p_telefono_secundario IN usuarios.telefono_secundario%TYPE,
        p_correo IN usuarios.correo%TYPE,
        p_contrasena IN usuarios.contrasena%TYPE,
        p_id_generado OUT NUMBER
    );

    -- Actualiza información de un usuario existente
    PROCEDURE actualizar_usuario(
        p_id_usuario IN usuarios.id_usuario%TYPE,
        p_id_direccion IN usuarios.id_direccion%TYPE,
        p_cedula IN usuarios.cedula%TYPE,
        p_primer_nombre IN usuarios.primer_nombre%TYPE,
        p_segundo_nombre IN usuarios.segundo_nombre%TYPE,
        p_apellido_paterno IN usuarios.apellido_paterno%TYPE,
        p_apellido_materno IN usuarios.apellido_materno%TYPE,
        p_telefono_principal IN usuarios.telefono_principal%TYPE,
        p_telefono_secundario IN usuarios.telefono_secundario%TYPE,
        p_correo IN usuarios.correo%TYPE,
        p_contrasena IN usuarios.contrasena%TYPE
    );

    -- Lista todos los usuarios activos del sistema
    PROCEDURE listar_usuarios_activos(
        p_cursor OUT SYS_REFCURSOR
    );

    -- Inactiva un usuario
    PROCEDURE eliminar_usuario(
        p_id_usuario IN usuarios.id_usuario%TYPE
    );

    -- Obtiene información completa del usuario con su dirección
    PROCEDURE obtener_usuario_con_direccion(
        p_id_usuario IN usuarios.id_usuario%TYPE,
        p_cursor OUT SYS_REFCURSOR
    );

END pkg_usuarios;
/

CREATE OR REPLACE PACKAGE BODY pkg_usuarios AS

    PROCEDURE login_usuario(
        p_correo IN usuarios.correo%TYPE,
        p_contrasena IN usuarios.contrasena%TYPE,
        p_cursor OUT SYS_REFCURSOR
    ) IS
        v_exist NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_exist
        FROM usuarios
        WHERE correo = p_correo
          AND contrasena = p_contrasena
          AND estado = 'A';

        IF v_exist = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Credenciales inválidas o usuario inactivo.');
        END IF;

        OPEN p_cursor FOR
            SELECT id_usuario, primer_nombre, apellido_paterno, id_rol
            FROM usuarios
            WHERE correo = p_correo
              AND contrasena = p_contrasena
              AND estado = 'A';

    END login_usuario;

    PROCEDURE crear_usuario(
        p_id_direccion IN usuarios.id_direccion%TYPE,
        p_cedula IN usuarios.cedula%TYPE,
        p_primer_nombre IN usuarios.primer_nombre%TYPE,
        p_segundo_nombre IN usuarios.segundo_nombre%TYPE,
        p_apellido_paterno IN usuarios.apellido_paterno%TYPE,
        p_apellido_materno IN usuarios.apellido_materno%TYPE,
        p_telefono_principal IN usuarios.telefono_principal%TYPE,
        p_telefono_secundario IN usuarios.telefono_secundario%TYPE,
        p_correo IN usuarios.correo%TYPE,
        p_contrasena IN usuarios.contrasena%TYPE,
        p_id_generado OUT NUMBER
    ) IS
    BEGIN
        p_id_generado := seq_usuarios.NEXTVAL;

        INSERT INTO usuarios(
            id_usuario, id_rol, id_direccion, cedula, primer_nombre,
            segundo_nombre, apellido_paterno, apellido_materno,
            telefono_principal, telefono_secundario, correo, contrasena,
            estado, fecha_registro
        ) VALUES (
            p_id_generado, 2, p_id_direccion, p_cedula, p_primer_nombre,
            p_segundo_nombre, p_apellido_paterno, p_apellido_materno,
            p_telefono_principal, p_telefono_secundario, p_correo, p_contrasena,
            'A', SYSDATE
        );

    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            RAISE_APPLICATION_ERROR(-20002, 'El correo o la cédula ya existen.');
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20003, 'Error al crear el usuario: ' || SQLERRM);
    END crear_usuario;

    PROCEDURE actualizar_usuario(
        p_id_usuario IN usuarios.id_usuario%TYPE,
        p_id_direccion IN usuarios.id_direccion%TYPE,
        p_cedula IN usuarios.cedula%TYPE,
        p_primer_nombre IN usuarios.primer_nombre%TYPE,
        p_segundo_nombre IN usuarios.segundo_nombre%TYPE,
        p_apellido_paterno IN usuarios.apellido_paterno%TYPE,
        p_apellido_materno IN usuarios.apellido_materno%TYPE,
        p_telefono_principal IN usuarios.telefono_principal%TYPE,
        p_telefono_secundario IN usuarios.telefono_secundario%TYPE,
        p_correo IN usuarios.correo%TYPE,
        p_contrasena IN usuarios.contrasena%TYPE
    ) IS
    BEGIN
        UPDATE usuarios
        SET id_direccion = p_id_direccion,
            cedula = p_cedula,
            primer_nombre = p_primer_nombre,
            segundo_nombre = p_segundo_nombre,
            apellido_paterno = p_apellido_paterno,
            apellido_materno = p_apellido_materno,
            telefono_principal = p_telefono_principal,
            telefono_secundario = p_telefono_secundario,
            correo = p_correo,
            contrasena = p_contrasena
        WHERE id_usuario = p_id_usuario;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20004, 'No se encontró el usuario a actualizar.');
        END IF;

    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            RAISE_APPLICATION_ERROR(-20005, 'Correo o cédula duplicada.');
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20006, 'Error al actualizar el usuario: ' || SQLERRM);
    END actualizar_usuario;

    PROCEDURE listar_usuarios_activos(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT u.id_usuario,
                   u.primer_nombre,
                   u.apellido_paterno,
                   u.correo,
                   u.telefono_principal,
                   u.fecha_registro,
                   d.direccion_completa,
                   c.nombre AS ciudad,
                   r.nombre AS rol
            FROM usuarios u
            LEFT JOIN direcciones d ON u.id_direccion = d.id_direccion
            LEFT JOIN ciudades c ON d.id_ciudad = c.id_ciudad
            LEFT JOIN roles r ON u.id_rol = r.id_rol
            WHERE u.estado = 'A';
    END listar_usuarios_activos;

    PROCEDURE eliminar_usuario(
        p_id_usuario IN usuarios.id_usuario%TYPE
    ) IS
    BEGIN
        UPDATE usuarios
        SET estado = 'I'
        WHERE id_usuario = p_id_usuario;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20007, 'No se encontró el usuario a eliminar.');
        END IF;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20008, 'Error al eliminar usuario: ' || SQLERRM);
    END eliminar_usuario;

    PROCEDURE obtener_usuario_con_direccion(
        p_id_usuario IN usuarios.id_usuario%TYPE,
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                u.id_usuario,
                u.cedula,
                u.id_direccion,
                u.primer_nombre,
                u.apellido_paterno,
                u.correo,
                u.telefono_principal,
                d.direccion_completa,
                d.barrio,
                d.codigo_postal,
                c.nombre AS ciudad_nombre,
                c.departamento
            FROM usuarios u
            LEFT JOIN direcciones d ON u.id_direccion = d.id_direccion
            LEFT JOIN ciudades c ON d.id_ciudad = c.id_ciudad
            WHERE u.id_usuario = p_id_usuario
              AND u.estado = 'A';

    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20099, 'Error al obtener usuario: ' || SQLERRM);
    END obtener_usuario_con_direccion;

END pkg_usuarios;
/