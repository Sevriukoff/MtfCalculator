# MtfCalculator
Консольное приложение, позволяющие сформировать график частотно контрастной характеристики изображения.

После запуска программы необходимо ввести полный путь к изображению для которого необходимо сформировать ЧКХ. После указания пути программа выполнит преобразования над изображением и сохранит изменённую копию рядом с исходным изображением. Далее программа экспортирует Excel файлы с графиками (ЧКХ, суммарная ESF, LSF, LSF с окном Хэмминга) в директорию с запущенным приложением. После экспорта можно разрешить программе построить графики функций в Excel документе. При соглашении откроется Excel документ и выполнится vba скрипт, который построит графики.

Приложение было разработано в рамках прохождения практики на предприятии ООО "Катод"

## Пример работы приложения
Входное изображение. Важно, что бы изображение было похоже на изображение ниже, иначе работоспособность программы не гарантируется. Возможны ошибки при бинаризации или при расчете графиков.

<a href="https://ibb.co/zJHKHRT"><img src="https://i.ibb.co/rsfnf6R/original-source.png" alt="original-source" border="0"></a>

Изменённое изображение

<a href="https://ibb.co/R7dYSpC"><img src="https://i.ibb.co/9GScv9q/original-source-changed.png" alt="original-source-changed" border="0"></a>

График суммарной ESF

<a href="https://ibb.co/kHVBCgM"><img src="https://i.ibb.co/r7K0rGv/SumESF.png" alt="SumESF" border="0"></a>

График LSF

<a href="https://ibb.co/hRrpkwG"><img src="https://i.ibb.co/QFhsZtW/LSF.png" alt="LSF" border="0"></a>

График LSF с окном Хэммининга

<a href="https://ibb.co/J2Pzrgj"><img src="https://i.ibb.co/pZp345j/LSFw.png" alt="LSFw" border="0"></a>

График ЧКХ

<a href="https://ibb.co/z2YbSkq"><img src="https://i.ibb.co/94L2qMQ/MTF.png" alt="MTF" border="0"></a>
