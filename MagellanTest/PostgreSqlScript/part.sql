CREATE DATABASE "Part";

\c "Part";

CREATE TABLE item (
    id          int         NOT NULL    primary key,
    item_name   varchar(50) NOT NULL,
    parent_item int         REFERENCES  item(id),
    cost        int         NOT NULL,
    req_date    date        NOT NULL
);

INSERT INTO item
VALUES 
(1, 'Item1', NULL, 500, '02-20-2024'),
(2, 'Sub1', 1, 200, '02-10-2024'),
(3, 'Sub2', 1, 300, '01-05-2024'),
(4, 'Sub3', 2, 300, '01-02-2024'),
(5, 'Sub4', 2, 400, '01-02-2024'),
(6, 'Item2', NULL, 600, '03-15-2024'),
(7, 'Sub1', 6, 200, '02-25-2024');

CREATE FUNCTION Get_Total_Cost_ID(parent_id int) RETURNS int
AS $$
BEGIN
    RETURN (SELECT SUM(cost)
                FROM (
                    -- cost of id
                    (
                        SELECT cost as cost 
                        FROM item 
                        WHERE id = parent_id
                    )
                    UNION ALL
                    -- cost of children with parent id
                    (
                        SELECT Get_Total_Cost_ID(id) as cost 
                        FROM item 
                        WHERE parent_item = parent_id
                    )
                )
            );
END;
$$
LANGUAGE 'plpgsql';

CREATE FUNCTION Get_Total_Cost(p_name varchar(50)) RETURNS int
AS $$
BEGIN
    IF EXISTS ( SELECT parent_item 
                FROM item 
                WHERE item_name = p_name 
                AND parent_item IS NULL) 
    THEN
        RETURN (SELECT Get_Total_Cost_ID(id)
                FROM item
                WHERE item_name = p_name
                AND parent_item IS NULL);
    ELSE
        RETURN NULL;
    END IF;
END;
$$
LANGUAGE 'plpgsql';


-- SELECT Get_Total_Cost('Item1');

-- SELECT Get_Total_Cost('Sub1');

-- SELECT Get_Total_Cost('dfgdfg');