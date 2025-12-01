-- ========================================
-- STYLECODE E-COMMERCE
-- Configuración de Usuarios y Roles
-- ========================================

SHOW CON_NAME;

-- ========================================
-- LIMPIEZA PREVIA
-- ========================================

DROP USER sc_admin CASCADE;
DROP USER sc_user CASCADE;
DROP ROLE stylecode_dba_role;
DROP ROLE stylecode_dev_role;

-- ========================================
-- CREACIÓN DE ROLES
-- ========================================

CREATE ROLE stylecode_dba_role;

GRANT CREATE SESSION TO stylecode_dba_role;
GRANT CREATE TABLE TO stylecode_dba_role;
GRANT CREATE VIEW TO stylecode_dba_role;
GRANT CREATE SEQUENCE TO stylecode_dba_role;
GRANT CREATE PROCEDURE TO stylecode_dba_role;
GRANT CREATE TRIGGER TO stylecode_dba_role;
GRANT CREATE SYNONYM TO stylecode_dba_role;
GRANT CREATE PUBLIC SYNONYM TO stylecode_dba_role;
GRANT DROP ANY TABLE TO stylecode_dba_role;
GRANT DROP ANY VIEW TO stylecode_dba_role;
GRANT DROP ANY SEQUENCE TO stylecode_dba_role;
GRANT DROP PUBLIC SYNONYM TO stylecode_dba_role;
GRANT ALTER ANY TABLE TO stylecode_dba_role;
GRANT ALTER ANY SEQUENCE TO stylecode_dba_role;
GRANT CREATE ANY INDEX TO stylecode_dba_role;
GRANT DROP ANY INDEX TO stylecode_dba_role;

CREATE ROLE stylecode_dev_role;

GRANT CREATE SESSION TO stylecode_dev_role;
GRANT SELECT ANY TABLE TO stylecode_dev_role;
GRANT INSERT ANY TABLE TO stylecode_dev_role;
GRANT UPDATE ANY TABLE TO stylecode_dev_role;
GRANT DELETE ANY TABLE TO stylecode_dev_role;
GRANT EXECUTE ANY PROCEDURE TO stylecode_dev_role;
GRANT CREATE SYNONYM TO stylecode_dev_role;
GRANT SELECT ANY DICTIONARY TO stylecode_dev_role;
GRANT SELECT ANY SEQUENCE TO stylecode_dev_role;

-- ========================================
-- CREACIÓN DE USUARIOS
-- ========================================

CREATE USER sc_admin IDENTIFIED BY Admin2025$SC
  DEFAULT TABLESPACE users
  TEMPORARY TABLESPACE temp
  QUOTA UNLIMITED ON users;

GRANT stylecode_dba_role TO sc_admin;
GRANT CONNECT, RESOURCE TO sc_admin;
GRANT DROP ANY SYNONYM TO sc_admin;
GRANT CREATE PUBLIC SYNONYM TO sc_admin;

CREATE USER sc_user IDENTIFIED BY UserDB2025$SC
  DEFAULT TABLESPACE users
  TEMPORARY TABLESPACE temp
  QUOTA UNLIMITED ON users;

GRANT stylecode_dev_role TO sc_user;
GRANT CONNECT TO sc_user;

-- ========================================
-- VERIFICACIÓN
-- ========================================

SELECT username, default_tablespace, account_status
FROM dba_users
WHERE username IN ('SC_ADMIN', 'SC_USER')
ORDER BY username;

SELECT role FROM dba_roles
WHERE role LIKE 'STYLECODE%'
ORDER BY role;

SELECT grantee, granted_role
FROM dba_role_privs
WHERE grantee IN ('SC_ADMIN', 'SC_USER')
ORDER BY grantee, granted_role;