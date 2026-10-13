CREATE OR REPLACE PROCEDURE public.create_user(
    IN p_name varchar(120),
    IN p_email varchar(120),
    IN p_password varchar(120),
    OUT p_id uuid
)
LANGUAGE plpgsql AS
$$
BEGIN
INSERT INTO public.users(name, email, password)
VALUES (p_name, p_email, p_password)
    RETURNING id INTO p_id;
END;
$$;

CREATE OR REPLACE PROCEDURE public.delete_user(
    IN p_id uuid
)
LANGUAGE plpgsql AS
$$
BEGIN
DELETE FROM public.users WHERE id = p_id;
END;
$$;

