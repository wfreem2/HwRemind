create role client with LOGIN PASSWORD 'password';

create table "Logins"
(
    id       serial
        constraint logins_pk
            primary key,
    email    varchar(60)  not null,
    password varchar(128) not null
);

alter table "Logins"
    owner to root;

grant select, usage on sequence "Logins_id_seq" to client;

create unique index logins_email_uindex
    on "Logins" (email);

grant delete, insert, references, select, truncate, update on "Logins" to client;

create table "RefreshTokens"
(
    token      char(88)                 not null
        constraint refreshtokens_pk
            primary key,
    "loginId"  integer
        constraint refreshtokens_logins_id_fk
            references "Logins"
            on update cascade on delete cascade,
    expiration timestamp with time zone not null
);

alter table "RefreshTokens"
    owner to root;

create unique index refreshtokens_token_uindex
    on "RefreshTokens" (token);

create unique index refreshtokens_login_id_uindex
    on "RefreshTokens" ("loginId");

grant delete, insert, references, select, truncate, update on "RefreshTokens" to client;

create table "Users"
(
    id           serial
        constraint users_pk
            primary key,
    "firstName"  varchar(64),
    "lastName"   varchar(64),
    "schoolName" varchar(80),
    "loginId"    integer
        constraint users_logins_id_fk
            references "Logins"
            on update cascade on delete cascade
);

alter table "Users"
    owner to root;

grant select, usage on sequence "Users_id_seq" to client;

create unique index users_loginid_uindex
    on "Users" ("loginId");

grant delete, insert, references, select, update on "Users" to client;

create table "Assignments"
(
    id          serial
        constraint assignments_pk
            primary key,
    name        varchar(128) not null,
    description text,
    "dueAt"     timestamp with time zone,
    "userId"    integer
        constraint assignments_users_id_fk
            references "Users"
            on update cascade on delete cascade
);

alter table "Assignments"
    owner to root;

grant select, usage on sequence "Assignments_id_seq" to client;

grant delete, insert, references, select, update on "Assignments" to client;

create function delete_expired_refresh_tokens() returns trigger
    language plpgsql
as
$$
BEGIN
        DELETE from "RefreshTokens"
            where expiration < now();
        RETURN NEW;
    END;
$$;

alter function delete_expired_refresh_tokens() owner to root;

create trigger delete_expired_refresh_tokens
    after insert
    on "RefreshTokens"
execute procedure delete_expired_refresh_tokens();