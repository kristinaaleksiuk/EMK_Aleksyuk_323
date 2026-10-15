select * from [dbo].[Покупатели]
where [ID заказа] > 3

select * from [dbo].[Оплата]
where [Статус оплаты] LIKE 'ОПЛАЧЕНО%';

select * from [dbo].[Покупатели]
where [ID заказа] = 3

select * from [dbo].[Оплата]
where [Сумма] > 1000

select * from [dbo].[Оплата]
where [Способ оплаты] LIKE 'КАРТА%';

select * from [dbo].[Товары]
where [Название] LIKE 'УКРОП%'; 

select * from [dbo].[Товары] where [Цена] <= 60;
