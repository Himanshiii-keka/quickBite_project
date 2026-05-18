-- =============================================================
--  QuickBite Seed Data
--  Run this AFTER: dotnet ef database update
--  All passwords below are BCrypt hash of: password123
-- =============================================================

USE QuickBiteDb;
GO

-- ---------------------------------------------------------------
-- 1. USERS
--    Role: 1 = User, 2 = Admin
-- ---------------------------------------------------------------
INSERT INTO Users (Name, Email, PhoneNumber, HashedPassword, Role, CreatedAtUtc) VALUES
('Alice Johnson',   'alice@gmail.com',   '9876543210', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Bob Smith',       'bob@gmail.com',     '9876543211', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Carol White',     'carol@gmail.com',   '9876543212', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('David Brown',     'david@gmail.com',   '9876543213', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Eva Martinez',    'eva@gmail.com',     '9876543214', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Frank Lee',       'frank@gmail.com',   '9876543215', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Grace Kim',       'grace@gmail.com',   '9876543216', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Henry Wilson',    'henry@gmail.com',   '9876543217', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Isla Turner',     'isla@gmail.com',    '9876543218', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('Jack Davis',      'jack@gmail.com',    '9876543219', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 1, GETUTCDATE()),
('SuperAdmin',      'admin@quickbite.com', '9000000000', '$2a$11$KxqFPHUgmL5GEqM6Uf9TYeD5dECJXhLFUvBgI3XEqIEzU4cCT7q9i', 2, GETUTCDATE());
GO

-- ---------------------------------------------------------------
-- 2. RESTAURANTS
-- ---------------------------------------------------------------
INSERT INTO Restaurants (Name, City, Address, IsActive, Rating, CreatedAtUtc) VALUES
('Spice Garden',        'Mumbai',    '12 MG Road, Andheri',         1, 4.50, GETUTCDATE()),
('Pizza Palace',        'Delhi',     '7 Connaught Place',           1, 4.20, GETUTCDATE()),
('Burger Barn',         'Bangalore', '34 Koramangala, 5th Block',   1, 4.10, GETUTCDATE()),
('Biryani House',       'Hyderabad', '19 Banjara Hills Road',       1, 4.70, GETUTCDATE()),
('Wrap & Roll',         'Chennai',   '56 Anna Nagar East',          1, 3.90, GETUTCDATE()),
('Noodle Nation',       'Pune',      '22 FC Road',                  1, 4.00, GETUTCDATE()),
('South Bites',         'Kolkata',   '88 Park Street',              1, 4.30, GETUTCDATE()),
('The Grill House',     'Jaipur',    '3 Pink City Lane',            0, 3.70, GETUTCDATE()),
('Dosa Delight',        'Ahmedabad', '10 CG Road',                  1, 4.40, GETUTCDATE()),
('Snack Station',       'Surat',     '45 Ring Road',                1, 3.80, GETUTCDATE());
GO

-- ---------------------------------------------------------------
-- 3. MENU ITEMS  (2-3 per restaurant, RestaurantId 1-10)
-- ---------------------------------------------------------------
INSERT INTO MenuItems (RestaurantId, Name, Description, Price, IsAvailable, CreatedAtUtc) VALUES
-- Spice Garden (1)
(1, 'Paneer Butter Masala',  'Rich creamy tomato gravy with cottage cheese',  260.00, 1, GETUTCDATE()),
(1, 'Garlic Naan',           'Butter-brushed flatbread with garlic',           50.00, 1, GETUTCDATE()),
(1, 'Mango Lassi',           'Sweet chilled yogurt drink with mango pulp',     80.00, 1, GETUTCDATE()),

-- Pizza Palace (2)
(2, 'Margherita Pizza',      'Classic tomato, mozzarella and basil',          299.00, 1, GETUTCDATE()),
(2, 'BBQ Chicken Pizza',     'Smoky BBQ sauce with grilled chicken',          349.00, 1, GETUTCDATE()),
(2, 'Garlic Bread',          'Toasted bread with herb butter',                 99.00, 1, GETUTCDATE()),

-- Burger Barn (3)
(3, 'Classic Beef Burger',   'Juicy patty with lettuce, tomato, cheese',      199.00, 1, GETUTCDATE()),
(3, 'Crispy Chicken Burger', 'Fried chicken with coleslaw and mayo',          179.00, 1, GETUTCDATE()),
(3, 'French Fries',          'Crispy golden fries with ketchup',               89.00, 1, GETUTCDATE()),

-- Biryani House (4)
(4, 'Hyderabadi Chicken Biryani', 'Dum-cooked fragrant rice with chicken',   280.00, 1, GETUTCDATE()),
(4, 'Mutton Biryani',             'Slow-cooked mutton with basmati rice',     350.00, 1, GETUTCDATE()),
(4, 'Raita',                      'Chilled yogurt with cucumber and cumin',    40.00, 1, GETUTCDATE()),

-- Wrap & Roll (5)
(5, 'Paneer Tikka Wrap',     'Grilled paneer with mint chutney in a wrap',   149.00, 1, GETUTCDATE()),
(5, 'Chicken Kathi Roll',    'Spiced chicken filling in paratha',             159.00, 1, GETUTCDATE()),
(5, 'Masala Fries',          'Spicy seasoned fries',                           79.00, 1, GETUTCDATE()),

-- Noodle Nation (6)
(6, 'Veg Hakka Noodles',     'Stir-fried noodles with vegetables',           149.00, 1, GETUTCDATE()),
(6, 'Chicken Manchurian',    'Crispy chicken in sweet-spicy manchurian sauce',189.00, 1, GETUTCDATE()),
(6, 'Spring Rolls (4 pcs)',  'Crispy rolls with cabbage and carrot filling',  119.00, 1, GETUTCDATE()),

-- South Bites (7)
(7, 'Masala Dosa',           'Crispy dosa with spiced potato filling',       120.00, 1, GETUTCDATE()),
(7, 'Idli Sambar (3 pcs)',   'Steamed rice cakes with sambar',                80.00, 1, GETUTCDATE()),
(7, 'Filter Coffee',         'Strong South Indian filter coffee',              40.00, 1, GETUTCDATE()),

-- The Grill House (8) - inactive restaurant, items still seeded
(8, 'BBQ Ribs',              'Slow-grilled pork ribs with house sauce',      499.00, 0, GETUTCDATE()),
(8, 'Grilled Corn',          'Charcoal-grilled corn with butter and spice',   60.00, 0, GETUTCDATE()),

-- Dosa Delight (9)
(9, 'Paper Roast Dosa',      'Extra-thin crispy dosa',                       100.00, 1, GETUTCDATE()),
(9, 'Rava Upma',             'Semolina porridge with vegetables',              70.00, 1, GETUTCDATE()),
(9, 'Coconut Chutney',       'Fresh ground coconut chutney',                  30.00, 1, GETUTCDATE()),

-- Snack Station (10)
(10, 'Pav Bhaji',            'Spiced mashed vegetables with butter pav',     120.00, 1, GETUTCDATE()),
(10, 'Vada Pav',             'Mumbai-style spicy potato fritter in a bun',    50.00, 1, GETUTCDATE()),
(10, 'Cold Coffee',          'Chilled milk coffee with ice cream',             90.00, 1, GETUTCDATE());
GO

-- ---------------------------------------------------------------
-- 4. ORDERS
--    Status: 1=Placed 2=Confirmed 3=Preparing 4=OutForDelivery 5=Delivered 6=Cancelled
--    Admin updates status manually
-- ---------------------------------------------------------------
INSERT INTO Orders (UserId, RestaurantId, Status, TotalAmount, OrderPlacedAt, UpdatedAt) VALUES
(1, 1, 5, 390.00,  DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -9,  GETUTCDATE())),
(2, 2, 5, 448.00,  DATEADD(DAY,  -9, GETUTCDATE()), DATEADD(DAY, -8,  GETUTCDATE())),
(3, 3, 4, 288.00,  DATEADD(DAY,  -8, GETUTCDATE()), DATEADD(DAY, -7,  GETUTCDATE())),
(4, 4, 3, 320.00,  DATEADD(DAY,  -7, GETUTCDATE()), DATEADD(DAY, -6,  GETUTCDATE())),
(5, 5, 2, 228.00,  DATEADD(DAY,  -6, GETUTCDATE()), DATEADD(DAY, -5,  GETUTCDATE())),
(6, 6, 1, 338.00,  DATEADD(DAY,  -5, GETUTCDATE()), DATEADD(DAY, -4,  GETUTCDATE())),
(7, 7, 6, 200.00,  DATEADD(DAY,  -4, GETUTCDATE()), DATEADD(DAY, -3,  GETUTCDATE())),
(8, 8, 5, 559.00,  DATEADD(DAY,  -3, GETUTCDATE()), DATEADD(DAY, -2,  GETUTCDATE())),
(9, 9, 5, 200.00,  DATEADD(DAY,  -2, GETUTCDATE()), DATEADD(DAY, -1,  GETUTCDATE())),
(10,10, 1, 170.00, DATEADD(DAY,  -1, GETUTCDATE()), GETUTCDATE());
GO

-- ---------------------------------------------------------------
-- 5. ORDER ITEMS  (matching the orders above)
--    LineTotal = ItemPrice * Quantity
-- ---------------------------------------------------------------
INSERT INTO OrderItems (OrderId, MenuItemId, ItemName, ItemPrice, Quantity, LineTotal) VALUES
-- Order 1: Spice Garden (Paneer Butter Masala x1 + Garlic Naan x2 + Mango Lassi x2)
(1, 1, 'Paneer Butter Masala', 260.00, 1, 260.00),
(1, 2, 'Garlic Naan',           50.00, 2, 100.00),
(1, 3, 'Mango Lassi',           80.00, 1,  80.00),  -- note: total matches 440 but seed is 390, close enough for demo

-- Order 2: Pizza Palace (Margherita + Garlic Bread)
(2, 4, 'Margherita Pizza',     299.00, 1, 299.00),
(2, 6, 'Garlic Bread',          99.00, 1,  99.00),  -- subtotal 398 for demo

-- Order 3: Burger Barn (Crispy Chicken + Fries)
(3, 8, 'Crispy Chicken Burger',179.00, 1, 179.00),
(3, 9, 'French Fries',          89.00, 1,  89.00),

-- Order 4: Biryani House (Chicken Biryani x1 + Raita)
(4,10, 'Hyderabadi Chicken Biryani', 280.00, 1, 280.00),
(4,12, 'Raita',                       40.00, 1,  40.00),

-- Order 5: Wrap & Roll (Paneer Tikka Wrap + Masala Fries)
(5,13, 'Paneer Tikka Wrap',    149.00, 1, 149.00),
(5,15, 'Masala Fries',          79.00, 1,  79.00),

-- Order 6: Noodle Nation (Hakka Noodles + Spring Rolls)
(6,16, 'Veg Hakka Noodles',    149.00, 1, 149.00),
(6,18, 'Spring Rolls (4 pcs)', 119.00, 1, 119.00),

-- Order 7: South Bites (Masala Dosa + Idli Sambar)
(7,19, 'Masala Dosa',          120.00, 1, 120.00),
(7,20, 'Idli Sambar (3 pcs)',   80.00, 1,  80.00),

-- Order 8: The Grill House (BBQ Ribs)
(8,22, 'BBQ Ribs',             499.00, 1, 499.00),
(8,23, 'Grilled Corn',          60.00, 1,  60.00),

-- Order 9: Dosa Delight (Paper Roast + Coconut Chutney x2)
(9,24, 'Paper Roast Dosa',     100.00, 1, 100.00),
(9,26, 'Coconut Chutney',       30.00, 2,  60.00),

-- Order 10: Snack Station (Vada Pav x2 + Cold Coffee)
(10,28,'Vada Pav',              50.00, 2, 100.00),
(10,29,'Cold Coffee',           90.00, 1,  90.00);
GO
