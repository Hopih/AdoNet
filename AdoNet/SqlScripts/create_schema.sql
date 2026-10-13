BEGIN;

CREATE TABLE public.users (
    id       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name     varchar(120) NOT NULL CHECK (length(btrim(name)) > 0),
    email    varchar(120) NOT NULL CHECK (length(btrim(email)) > 0),
    password varchar(120) NOT NULL CHECK (length(btrim(password)) > 0)
);

CREATE TABLE public.projects (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    uuid NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    address    varchar(200) NOT NULL CHECK (length(btrim(address)) > 0),
    start_date date NOT NULL DEFAULT current_date,
    budget     numeric(12, 2) NOT NULL CHECK (budget >= 0)
);

CREATE TABLE public.stages (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id uuid NOT NULL REFERENCES public.projects(id) ON DELETE CASCADE,
    title      varchar(120) NOT NULL CHECK (length(btrim(title)) > 0),
    cost       numeric(12, 2) NOT NULL CHECK (cost >= 0),
    status     varchar(30) NOT NULL
);

COMMIT;
