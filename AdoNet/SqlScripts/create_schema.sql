BEGIN;
CREATE TABLE public.users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name varchar(120) NOT NULL CHECK ( length(btrin(name>0)) )
    email varchar(120) NOT NULL CHECK ( length(btrin(email>0)) )
    password varchar(120) NOT NULL CHECK ( length(btrin(password>0)) )
)