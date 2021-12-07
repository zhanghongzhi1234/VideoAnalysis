-- phpMyAdmin SQL Dump
-- version 4.1.14
-- http://www.phpmyadmin.net
--
-- Host: 127.0.0.1
-- Generation Time: Aug 17, 2017 at 10:43 AM
-- Server version: 5.6.17
-- PHP Version: 5.5.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `videoanalysis`
--
CREATE DATABASE IF NOT EXISTS `videoanalysis` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
USE `videoanalysis`;

-- --------------------------------------------------------

--
-- Table structure for table `template`
--

DROP TABLE IF EXISTS `template`;
CREATE TABLE IF NOT EXISTS `template` (
  `pkey` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `type` varchar(20) DEFAULT NULL,
  `note` mediumtext NOT NULL,
  PRIMARY KEY (`pkey`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=19 ;

--
-- Dumping data for table `template`
--

INSERT INTO `template` (`pkey`, `name`, `type`, `note`) VALUES
(7, 'hongzhi1', 'VIP', 'asdfasdf'),
(8, 'limin', 'VIP', 'limin'),
(16, 'Kimshen', 'VIP', 'Project Manager'),
(17, 'Name', 'VIP', 'NOTE'),
(18, 'Name', 'BLACKLIST', 'NOTE');

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
